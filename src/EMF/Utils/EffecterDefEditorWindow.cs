using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using Verse;

namespace EMF
{


    public class EffecterDefEditorWindow : Window
    {
        private List<EffecterDef> allEffecterDefs;
        private EffecterDef selectedEffecterDef;
        private EffecterDef workingCopy;

        private string searchText = "";
        private Vector2 scrollPosition = Vector2.zero;
        private float viewHeight;
        private bool showBaseFields = true;
        private bool showSubEffecters = true;
        private Dictionary<string, bool> subEffecterFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, Vector2> subEffecterScrollPositions = new Dictionary<string, Vector2>();

        private const float FIELD_HEIGHT = 30f;
        private const float LABEL_WIDTH = 200f;
        private const float SECTION_PADDING = 35f;
        private const float BUTTON_WIDTH = 100f;
        private const float BUTTON_SPACING = 10f;

        public override Vector2 InitialSize => new Vector2(900f, 700f);

        public EffecterDefEditorWindow()
        {
            this.forcePause = false;
            this.doCloseX = true;
            this.doCloseButton = false;
            this.closeOnClickedOutside = false;
            this.absorbInputAroundWindow = false;
            this.resizeable = true;
            this.draggable = true;
            this.onlyOneOfTypeAllowed = true;
            this.preventCameraMotion = false;
            allEffecterDefs = DefDatabase<EffecterDef>.AllDefs.OrderBy(def => def.defName).ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect toolbarRect = new Rect(0f, 0f, inRect.width, 30f);
            Rect contentRect = new Rect(0f, 35f, inRect.width, inRect.height - 35f - 35f);
            Rect bottomButtonRect = new Rect(0f, inRect.height - 30f, inRect.width, 30f);

            DrawToolbar(toolbarRect);

            if (workingCopy != null)
            {
                Widgets.DrawMenuSection(contentRect);
                DrawContentSection(contentRect.ContractedBy(5f));
            }
            else
            {
                GUI.color = Color.gray;
                Widgets.Label(contentRect, "Select an EffecterDef from the dropdown above to begin editing.");
                GUI.color = Color.white;
            }

            DrawBottomButtons(bottomButtonRect);
        }

        private void DrawToolbar(Rect rect)
        {
            Rect searchRect = new Rect(rect.x, rect.y, 200f, rect.height);
            Rect dropdownRect = new Rect(searchRect.xMax + 10f, rect.y, 300f, rect.height);
            Rect labelRect = new Rect(dropdownRect.xMax + 10f, rect.y, 200f, rect.height);

            searchText = Widgets.TextField(searchRect, searchText);
            if (searchText.NullOrEmpty())
            {
                GUI.color = Color.gray;
                Widgets.Label(searchRect, "Search EffecterDefs...");
                GUI.color = Color.white;
            }

            List<EffecterDef> filteredDefs = allEffecterDefs;
            if (!searchText.NullOrEmpty())
            {
                filteredDefs = allEffecterDefs.Where(def => def.defName.ToLower().Contains(searchText.ToLower())).ToList();
            }

            if (Widgets.ButtonText(dropdownRect, selectedEffecterDef?.defName ?? "Select an EffecterDef"))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach (EffecterDef def in filteredDefs)
                {
                    options.Add(new FloatMenuOption(def.defName, () => {

                        SetWorkingCopy(def);
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            GUI.color = Color.gray;
            Widgets.Label(labelRect, $"Showing {filteredDefs.Count} of {allEffecterDefs.Count} EffecterDefs");
            GUI.color = Color.white;
        }


        private void SetWorkingCopy(EffecterDef effecterDef)
        {
            selectedEffecterDef = effecterDef;
            workingCopy = DeepCopyEffecterDef(effecterDef);
            ResetSubEffecterFoldouts();
        }

        private void ResetSubEffecterFoldouts()
        {
            subEffecterFoldouts.Clear();
            if (workingCopy.children != null)
            {
                foreach (var subDef in workingCopy.children)
                {
                    string key = subDef.GetHashCode().ToString();
                    subEffecterFoldouts[key] = true;
                    subEffecterScrollPositions[key] = Vector2.zero;
                }
            }
        }

        private void DrawContentSection(Rect rect)
        {
            Rect viewRect = new Rect(0f, 0f, rect.width - 20f, viewHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float curY = 0f;

            curY = DrawSectionHeader(curY, viewRect.width, "EffecterDef Base Properties", ref showBaseFields);

            if (showBaseFields)
            {
                curY = DrawEffecterDefBaseFields(new Rect(10f, curY, viewRect.width - 10f, 1000f), curY);
                curY += SECTION_PADDING;
            }

            curY = DrawSectionHeader(curY, viewRect.width, "SubEffecterDefs", ref showSubEffecters);

            if (showSubEffecters)
            {
                if (workingCopy.children == null)
                {
                    workingCopy.children = new List<SubEffecterDef>();
                }

                Rect addSubEffecterRect = new Rect(10f, curY, 200f, FIELD_HEIGHT);
                if (Widgets.ButtonText(addSubEffecterRect, "Add SubEffecterDef"))
                {
                    AddNewSubEffecter();
                }
                curY += FIELD_HEIGHT + 5f;

                for (int i = 0; i < workingCopy.children.Count; i++)
                {
                    SubEffecterDef subDef = workingCopy.children[i];
                    curY = DrawSubEffecterSection(curY, viewRect.width, subDef, i);
                }
            }

            viewHeight = curY + 50f;
            Widgets.EndScrollView();
        }

        private void AddNewSubEffecter()
        {
            SubEffecterDef newSubDef = new SubEffecterDef();
            newSubDef.subEffecterClass = typeof(SubEffecter);
            newSubDef.burstCount = new IntRange(1, 1);
            newSubDef.ticksBetweenMotes = 40;
            newSubDef.color = Color.white;
            workingCopy.children.Add(newSubDef);

            string key = newSubDef.GetHashCode().ToString();
            subEffecterFoldouts[key] = true;
            subEffecterScrollPositions[key] = Vector2.zero;
        }

        private float DrawSectionHeader(float curY, float width, string label, ref bool expanded)
        {
            Rect headerRect = new Rect(0f, curY, width, FIELD_HEIGHT);
            if (Widgets.ButtonTextSubtle(headerRect, expanded ? "▼ " + label : "► " + label))
            {
                expanded = !expanded;
            }
            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawSubEffecterSection(float curY, float width, SubEffecterDef subDef, int index)
        {
            string key = subDef.GetHashCode().ToString();
            if (!subEffecterFoldouts.ContainsKey(key))
            {
                subEffecterFoldouts[key] = false;
            }
            if (!subEffecterScrollPositions.ContainsKey(key))
            {
                subEffecterScrollPositions[key] = Vector2.zero;
            }

            Rect headerRect = new Rect(10f, curY, width - 10f, FIELD_HEIGHT);
            Rect removeRect = new Rect(width - 80f, curY, 70f, FIELD_HEIGHT);

            GUI.color = new Color(0.8f, 0.8f, 0.8f);
            Widgets.DrawBox(headerRect);
            GUI.color = Color.white;

            string headerText = $"SubEffecter {index + 1}" + (subDef.moteDef != null ? $" - Mote: {subDef.moteDef.defName}" : "");
            Rect textRect = new Rect(headerRect.x + 5f, headerRect.y + 5f, headerRect.width - 90f, headerRect.height - 10f);

            if (Widgets.ButtonInvisible(textRect))
            {
                subEffecterFoldouts[key] = !subEffecterFoldouts[key];
            }

            GUI.color = subEffecterFoldouts[key] ? Color.white : Color.gray;
            Widgets.Label(textRect, headerText);
            GUI.color = Color.white;

            if (Widgets.ButtonText(removeRect, "Remove"))
            {
                workingCopy.children.RemoveAt(index);
                return curY;
            }

            curY += FIELD_HEIGHT + 5f;

            if (subEffecterFoldouts[key])
            {
                float subDefHeight = DrawSubEffecterDefFields(new Rect(20f, curY, width - 20f, 1000f), curY, subDef);
                curY += subDefHeight + 15f;
            }

            return curY;
        }

        private float DrawEffecterDefBaseFields(Rect rect, float curY)
        {
            float startY = curY;

            curY = DrawTextField(rect, curY, "DefName:", ref workingCopy.defName);
            curY = DrawFloatField(rect, curY, "Position Radius:", ref workingCopy.positionRadius);
            curY = DrawFloatRangeField(rect, curY, "Offset Towards Target (Min/Max):", ref workingCopy.offsetTowardsTarget);
            curY = DrawIntField(rect, curY, "Maintain Ticks:", ref workingCopy.maintainTicks);
            curY = DrawFloatField(rect, curY, "Random Weight:", ref workingCopy.randomWeight);
            curY = DrawEnumField<EffecterDef.OffsetMode>(rect, curY, "Offset Mode:", workingCopy.offsetMode.ToString(),
                (string selected) => {
                    workingCopy.offsetMode = (EffecterDef.OffsetMode)Enum.Parse(typeof(EffecterDef.OffsetMode), selected);
                },
                Enum.GetNames(typeof(EffecterDef.OffsetMode)));

            return curY - startY;
        }

        private float DrawSubEffecterDefFields(Rect rect, float curY, SubEffecterDef subDef)
        {
            float startY = curY;

            // Basic Settings
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, curY, rect.width, FIELD_HEIGHT), "Basic Settings");
            Text.Font = GameFont.Small;
            curY += FIELD_HEIGHT + 5f;

            curY = DrawTypeSelectionField(rect, curY, "SubEffecter Class:", subDef.subEffecterClass?.Name ?? "Select Class",
                (Type selected) => {
                    subDef.subEffecterClass = selected;
                },
                GenTypes.AllTypes.Where(t => typeof(SubEffecter).IsAssignableFrom(t) && !t.IsAbstract).ToList());

            curY = DrawIntRangeField(rect, curY, "Burst Count (Min/Max):", ref subDef.burstCount);
            curY = DrawIntField(rect, curY, "Ticks Between Motes:", ref subDef.ticksBetweenMotes);
            curY = DrawIntField(rect, curY, "Max Mote Count:", ref subDef.maxMoteCount);
            curY = DrawIntField(rect, curY, "Initial Delay Ticks:", ref subDef.initialDelayTicks);
            curY = DrawIntField(rect, curY, "Lifespan Max Ticks:", ref subDef.lifespanMaxTicks);
            curY = DrawFloatField(rect, curY, "Chance Per Tick:", ref subDef.chancePerTick);
            curY = DrawIntField(rect, curY, "Chance Period Ticks:", ref subDef.chancePeriodTicks);

            // Position Settings
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, curY, rect.width, FIELD_HEIGHT), "Position Settings");
            Text.Font = GameFont.Small;
            curY += FIELD_HEIGHT + 5f;

            curY = DrawEnumField<MoteSpawnLocType>(rect, curY, "Spawn Location Type:", subDef.spawnLocType.ToString(),
                (string selected) => {
                    subDef.spawnLocType = (MoteSpawnLocType)Enum.Parse(typeof(MoteSpawnLocType), selected);
                },
                Enum.GetNames(typeof(MoteSpawnLocType)));

            curY = DrawFloatField(rect, curY, "Position Lerp Factor:", ref subDef.positionLerpFactor);
            curY = DrawVector3Field(rect, curY, "Position Offset (X/Y/Z):", ref subDef.positionOffset);
            curY = DrawFloatField(rect, curY, "Position Radius:", ref subDef.positionRadius);
            curY = DrawFloatField(rect, curY, "Position Radius Min:", ref subDef.positionRadiusMin);

            if (subDef.positionDimensions.HasValue)
            {
                Vector3 dimensions = subDef.positionDimensions.Value;
                curY = DrawVector3Field(rect, curY, "Position Dimensions:", ref dimensions);
                subDef.positionDimensions = dimensions;
            }
            else
            {
                Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
                Rect buttonRect = new Rect(rect.x + LABEL_WIDTH, curY, 150f, FIELD_HEIGHT);

                Widgets.Label(labelRect, "Position Dimensions:");
                if (Widgets.ButtonText(buttonRect, "Add Dimensions"))
                {
                    subDef.positionDimensions = Vector3.one;
                }
                curY += FIELD_HEIGHT + 5f;
            }

            curY = DrawBoolField(rect, curY, "Attach To Spawn Thing:", ref subDef.attachToSpawnThing);
            curY = DrawFloatField(rect, curY, "Avoid Last Position Radius:", ref subDef.avoidLastPositionRadius);

            curY = DrawEnumField<AttachPointType>(rect, curY, "Attach Point Type:", subDef.attachPoint.ToString(),
                (string selected) => {
                    subDef.attachPoint = (AttachPointType)Enum.Parse(typeof(AttachPointType), selected);
                },
                Enum.GetNames(typeof(AttachPointType)));

            // Visual Settings
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, curY, rect.width, FIELD_HEIGHT), "Visual Settings");
            Text.Font = GameFont.Small;
            curY += FIELD_HEIGHT + 5f;

            curY = DrawMoteDefField(rect, curY, "Mote Definition:", subDef);

            curY = DrawDefSelectionField(rect, curY, "Fleck Definition:", subDef.fleckDef?.defName ?? "Select FleckDef",
                (FleckDef selected) => {
                    subDef.fleckDef = selected;
                },
                DefDatabase<FleckDef>.AllDefs.OrderBy(d => d.defName).ToList(),
                true);

            Rect colorLabelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect colorFieldRect = new Rect(rect.x + LABEL_WIDTH, curY, rect.width - LABEL_WIDTH, FIELD_HEIGHT);

            Widgets.Label(colorLabelRect, "Color:");
            subDef.color = ColorWidgets.ColorField(colorFieldRect, subDef.color);
            curY += FIELD_HEIGHT + 5f;

            curY = DrawFloatRangeField(rect, curY, "Angle:", ref subDef.angle);
            curY = DrawBoolField(rect, curY, "Absolute Angle:", ref subDef.absoluteAngle);
            curY = DrawBoolField(rect, curY, "Use Target A Initial Rotation:", ref subDef.useTargetAInitialRotation);
            curY = DrawBoolField(rect, curY, "Use Target B Initial Rotation:", ref subDef.useTargetBInitialRotation);
            curY = DrawBoolField(rect, curY, "Fleck Uses Angle For Velocity:", ref subDef.fleckUsesAngleForVelocity);
            curY = DrawBoolField(rect, curY, "Rotate Towards Target Center:", ref subDef.rotateTowardsTargetCenter);
            curY = DrawBoolField(rect, curY, "Use Target A Body Angle:", ref subDef.useTargetABodyAngle);
            curY = DrawBoolField(rect, curY, "Use Target B Body Angle:", ref subDef.useTargetBBodyAngle);

            // Motion Settings
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, curY, rect.width, FIELD_HEIGHT), "Motion Settings");
            Text.Font = GameFont.Small;
            curY += FIELD_HEIGHT + 5f;

            curY = DrawFloatRangeField(rect, curY, "Speed:", ref subDef.speed);
            curY = DrawFloatRangeField(rect, curY, "Rotation:", ref subDef.rotation);
            curY = DrawFloatRangeField(rect, curY, "Rotation Rate:", ref subDef.rotationRate);
            curY = DrawFloatRangeField(rect, curY, "Scale:", ref subDef.scale);
            curY = DrawFloatRangeField(rect, curY, "Air Time:", ref subDef.airTime);

            // Sound Settings
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, curY, rect.width, FIELD_HEIGHT), "Sound Settings");
            Text.Font = GameFont.Small;
            curY += FIELD_HEIGHT + 5f;

            curY = DrawDefSelectionField(rect, curY, "Sound Definition:", subDef.soundDef?.defName ?? "Select SoundDef",
                (SoundDef selected) => {
                    subDef.soundDef = selected;
                },
                DefDatabase<SoundDef>.AllDefs.OrderBy(d => d.defName).ToList(),
                true);

            curY = DrawIntRangeField(rect, curY, "Intermittent Sound Interval:", ref subDef.intermittentSoundInterval);
            curY = DrawIntField(rect, curY, "Ticks Before Sustainer Start:", ref subDef.ticksBeforeSustainerStart);

            // Orbit Settings
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, curY, rect.width, FIELD_HEIGHT), "Orbit Settings");
            Text.Font = GameFont.Small;
            curY += FIELD_HEIGHT + 5f;

            curY = DrawBoolField(rect, curY, "Orbit Origin:", ref subDef.orbitOrigin);

            if (subDef.orbitSpeed != null)
            {
                curY = DrawFloatRangeField(rect, curY, "Orbit Speed:", ref subDef.orbitSpeed);
            }
            else
            {
                Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
                Rect buttonRect = new Rect(rect.x + LABEL_WIDTH, curY, 150f, FIELD_HEIGHT);

                Widgets.Label(labelRect, "Orbit Speed:");
                if (Widgets.ButtonText(buttonRect, "Add Orbit Speed"))
                {
                    subDef.orbitSpeed = new FloatRange(0f, 0f);
                }
                curY += FIELD_HEIGHT + 5f;
            }

            curY = DrawFloatField(rect, curY, "Orbit Snap Strength:", ref subDef.orbitSnapStrength);

            // Special Behavior
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, curY, rect.width, FIELD_HEIGHT), "Special Behavior");
            Text.Font = GameFont.Small;
            curY += FIELD_HEIGHT + 5f;

            curY = DrawBoolField(rect, curY, "Make Mote On Subtrigger:", ref subDef.makeMoteOnSubtrigger);
            curY = DrawBoolField(rect, curY, "Destroy Mote On Cleanup:", ref subDef.destroyMoteOnCleanup);

            if (subDef.cameraShake != null)
            {
                curY = DrawFloatRangeField(rect, curY, "Camera Shake:", ref subDef.cameraShake);
            }
            else
            {
                Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
                Rect buttonRect = new Rect(rect.x + LABEL_WIDTH, curY, 150f, FIELD_HEIGHT);

                Widgets.Label(labelRect, "Camera Shake:");
                if (Widgets.ButtonText(buttonRect, "Add Camera Shake"))
                {
                    subDef.cameraShake = new FloatRange(0f, 0f);
                }
                curY += FIELD_HEIGHT + 5f;
            }

            curY = DrawFloatField(rect, curY, "Distance Attenuation Scale:", ref subDef.distanceAttenuationScale);
            curY = DrawFloatField(rect, curY, "Distance Attenuation Max:", ref subDef.distanceAttenuationMax);
            curY = DrawFloatField(rect, curY, "Random Weight:", ref subDef.randomWeight);
            curY = DrawBoolField(rect, curY, "Sub Trigger On Spawn:", ref subDef.subTriggerOnSpawn);

            // Child SubEffecters
            if (subDef.children != null && subDef.children.Count > 0)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(rect.x, curY, rect.width, FIELD_HEIGHT), "Child SubEffecters");
                Text.Font = GameFont.Small;
                curY += FIELD_HEIGHT + 5f;

                Rect addChildRect = new Rect(rect.x, curY, 200f, FIELD_HEIGHT);
                if (Widgets.ButtonText(addChildRect, "Add Child SubEffecter"))
                {
                    if (subDef.children == null)
                    {
                        subDef.children = new List<SubEffecterDef>();
                    }

                    SubEffecterDef newChild = new SubEffecterDef();
                    newChild.subEffecterClass = typeof(SubEffecter);
                    newChild.burstCount = new IntRange(1, 1);
                    newChild.ticksBetweenMotes = 40;
                    newChild.color = Color.white;
                    subDef.children.Add(newChild);
                }
                curY += FIELD_HEIGHT + 5f;

                GUI.color = Color.yellow;
                Widgets.Label(new Rect(rect.x, curY, rect.width, FIELD_HEIGHT), "Warning: Nested SubEffecters are not fully editable here");
                GUI.color = Color.white;
                curY += FIELD_HEIGHT + 5f;

                for (int i = 0; i < subDef.children.Count; i++)
                {
                    Rect childLabelRect = new Rect(rect.x, curY, rect.width - 70f, FIELD_HEIGHT);
                    Rect childRemoveRect = new Rect(rect.x + rect.width - 70f, curY, 70f, FIELD_HEIGHT);

                    Widgets.Label(childLabelRect, $"Child SubEffecter {i + 1}");
                    if (Widgets.ButtonText(childRemoveRect, "Remove"))
                    {
                        subDef.children.RemoveAt(i);
                        i--;
                        continue;
                    }
                    curY += FIELD_HEIGHT + 5f;
                }
            }
            else
            {
                Rect addChildRect = new Rect(rect.x, curY, 200f, FIELD_HEIGHT);
                if (Widgets.ButtonText(addChildRect, "Add Child SubEffecter"))
                {
                    if (subDef.children == null)
                    {
                        subDef.children = new List<SubEffecterDef>();
                    }

                    SubEffecterDef newChild = new SubEffecterDef();
                    newChild.subEffecterClass = typeof(SubEffecter);
                    newChild.burstCount = new IntRange(1, 1);
                    newChild.ticksBetweenMotes = 40;
                    newChild.color = Color.white;
                    subDef.children.Add(newChild);
                }
                curY += FIELD_HEIGHT + 5f;
            }

            return curY - startY;
        }

        private float DrawBoolField(Rect rect, float curY, string label, ref bool value)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect checkRect = new Rect(rect.x + LABEL_WIDTH, curY, 30f, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);
            Widgets.Checkbox(checkRect.x, checkRect.y + (FIELD_HEIGHT - 24f) / 2, ref value);

            return curY + FIELD_HEIGHT + 5f;
        }

        private void DrawBottomButtons(Rect rect)
        {
            if (workingCopy == null)
            {
                if (Widgets.ButtonText(new Rect(rect.width - BUTTON_WIDTH, rect.y, BUTTON_WIDTH, rect.height), "Close"))
                {
                    Close();
                }
                return;
            }

            float rightEdge = rect.width;

            Rect closeRect = new Rect(rightEdge - BUTTON_WIDTH, rect.y, BUTTON_WIDTH, rect.height);
            rightEdge -= BUTTON_WIDTH + BUTTON_SPACING;

            Rect exportRect = new Rect(rightEdge - BUTTON_WIDTH, rect.y, BUTTON_WIDTH, rect.height);
            rightEdge -= BUTTON_WIDTH + BUTTON_SPACING;

            Rect spawnMaintainedRect = new Rect(rightEdge - BUTTON_WIDTH - 40, rect.y, BUTTON_WIDTH + 40, rect.height);
            rightEdge -= BUTTON_WIDTH + 40 + BUTTON_SPACING;

            Rect spawnRect = new Rect(rightEdge - BUTTON_WIDTH, rect.y, BUTTON_WIDTH, rect.height);

            if (Widgets.ButtonText(spawnRect, "Spawn"))
            {
                SpawnEffecter(false);
            }

            if (Widgets.ButtonText(spawnMaintainedRect, "Spawn Maintained"))
            {
                SpawnEffecter(true);
            }

            if (Widgets.ButtonText(exportRect, "Export"))
            {
                GUIUtility.systemCopyBuffer = EffecterDefToXmlString(workingCopy);
                Messages.Message("Copied to clipboard", MessageTypeDefOf.NeutralEvent);
            }

            if (Widgets.ButtonText(closeRect, "Close"))
            {
                Close();
            }
        }

        private void SpawnEffecter(bool maintained)
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true,
                canTargetPawns = true,
            },
            (LocalTargetInfo target) =>
            {

                if (target.HasThing && target.Thing is Pawn pawn)
                {
                    Effecter effecter = workingCopy.SpawnAttached(pawn, pawn.Map);
                    effecter.Trigger(pawn, pawn, -1);
                    if (maintained)
                    {
                        Find.CurrentMap.effecterMaintainer.AddEffecterToMaintain(effecter, pawn.Position, 1000);
                    }
                    else
                    {
                        effecter.Cleanup();
                    }
                }
                else
                {
                    if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                    {
                        Effecter effecter = workingCopy.Spawn();
                        TargetInfo targetInfo = new TargetInfo(target.Cell, Find.CurrentMap, false);
                        effecter.Trigger(targetInfo, targetInfo, -1);
                        if (maintained)
                        {
                            Find.CurrentMap.effecterMaintainer.AddEffecterToMaintain(effecter, target.Cell, 1000);
                        }
                        else
                        {
                            effecter.Cleanup();
                        }
                    }
                }

            });
        }

        private float DrawTextField(Rect rect, float curY, string label, ref string value)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect fieldRect = new Rect(rect.x + LABEL_WIDTH, curY, rect.width - LABEL_WIDTH, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);
            value = Widgets.TextField(fieldRect, value);

            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawIntField(Rect rect, float curY, string label, ref int value)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect fieldRect = new Rect(rect.x + LABEL_WIDTH, curY, rect.width - LABEL_WIDTH, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);
            string buffer = value.ToString();
            Widgets.TextFieldNumeric(fieldRect, ref value, ref buffer);

            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawFloatField(Rect rect, float curY, string label, ref float value)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect fieldRect = new Rect(rect.x + LABEL_WIDTH, curY, rect.width - LABEL_WIDTH, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);
            string buffer = value.ToString();
            Widgets.TextFieldNumeric(fieldRect, ref value, ref buffer);

            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawFloatRangeField(Rect rect, float curY, string label, ref FloatRange range)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect minRect = new Rect(rect.x + LABEL_WIDTH, curY, (rect.width - LABEL_WIDTH) / 2 - 5f, FIELD_HEIGHT);
            Rect maxRect = new Rect(rect.x + LABEL_WIDTH + (rect.width - LABEL_WIDTH) / 2 + 5f, curY, (rect.width - LABEL_WIDTH) / 2 - 5f, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);

            string minBuffer = range.min.ToString();
            float minVal = range.min;
            Widgets.TextFieldNumeric(minRect, ref minVal, ref minBuffer);

            string maxBuffer = range.max.ToString();
            float maxVal = range.max;
            Widgets.TextFieldNumeric(maxRect, ref maxVal, ref maxBuffer);

            range = new FloatRange(minVal, maxVal);

            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawIntRangeField(Rect rect, float curY, string label, ref IntRange range)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect minRect = new Rect(rect.x + LABEL_WIDTH, curY, (rect.width - LABEL_WIDTH) / 2 - 5f, FIELD_HEIGHT);
            Rect maxRect = new Rect(rect.x + LABEL_WIDTH + (rect.width - LABEL_WIDTH) / 2 + 5f, curY, (rect.width - LABEL_WIDTH) / 2 - 5f, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);

            string minBuffer = range.min.ToString();
            int minVal = range.min;
            Widgets.TextFieldNumeric(minRect, ref minVal, ref minBuffer);

            string maxBuffer = range.max.ToString();
            int maxVal = range.max;
            Widgets.TextFieldNumeric(maxRect, ref maxVal, ref maxBuffer);

            range = new IntRange(minVal, maxVal);

            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawVector3Field(Rect rect, float curY, string label, ref Vector3 vector)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect xRect = new Rect(rect.x + LABEL_WIDTH, curY, (rect.width - LABEL_WIDTH) / 3 - 5f, FIELD_HEIGHT);
            Rect yRect = new Rect(rect.x + LABEL_WIDTH + (rect.width - LABEL_WIDTH) / 3 + 5f, curY, (rect.width - LABEL_WIDTH) / 3 - 5f, FIELD_HEIGHT);
            Rect zRect = new Rect(rect.x + LABEL_WIDTH + 2 * ((rect.width - LABEL_WIDTH) / 3 + 5f), curY, (rect.width - LABEL_WIDTH) / 3 - 5f, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);

            string xBuffer = vector.x.ToString();
            float x = vector.x;
            Widgets.TextFieldNumeric(xRect, ref x, ref xBuffer);

            string yBuffer = vector.y.ToString();
            float y = vector.y;
            Widgets.TextFieldNumeric(yRect, ref y, ref yBuffer);

            string zBuffer = vector.z.ToString();
            float z = vector.z;
            Widgets.TextFieldNumeric(zRect, ref z, ref zBuffer);

            vector = new Vector3(x, y, z);

            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawEnumField<T>(Rect rect, float curY, string label, string currentValue, Action<string> setter, string[] options)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect fieldRect = new Rect(rect.x + LABEL_WIDTH, curY, rect.width - LABEL_WIDTH, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);

            if (Widgets.ButtonText(fieldRect, currentValue))
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
                foreach (string option in options)
                {
                    menuOptions.Add(new FloatMenuOption(option, () => setter(option)));
                }
                Find.WindowStack.Add(new FloatMenu(menuOptions));
            }

            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawTypeSelectionField(Rect rect, float curY, string label, string currentValue, Action<Type> setter, List<Type> options)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect fieldRect = new Rect(rect.x + LABEL_WIDTH, curY, rect.width - LABEL_WIDTH, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);

            if (Widgets.ButtonText(fieldRect, currentValue))
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
                foreach (Type type in options)
                {
                    menuOptions.Add(new FloatMenuOption(type.Name, () => setter(type)));
                }
                Find.WindowStack.Add(new FloatMenu(menuOptions));
            }

            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawDefSelectionField<T>(Rect rect, float curY, string label, string currentValue, Action<T> setter, List<T> options, bool allowNone = false) where T : Def
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect fieldRect = new Rect(rect.x + LABEL_WIDTH, curY, rect.width - LABEL_WIDTH, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);

            if (Widgets.ButtonText(fieldRect, currentValue))
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();

                if (allowNone)
                {
                    menuOptions.Add(new FloatMenuOption("None", () => setter(null)));
                }

                foreach (T def in options)
                {
                    menuOptions.Add(new FloatMenuOption(def.defName, () => setter(def)));
                }

                Find.WindowStack.Add(new FloatMenu(menuOptions));
            }

            return curY + FIELD_HEIGHT + 5f;
        }

        private float DrawMoteDefField(Rect rect, float curY, string label, SubEffecterDef subDef)
        {
            Rect labelRect = new Rect(rect.x, curY, LABEL_WIDTH, FIELD_HEIGHT);
            Rect fieldRect = new Rect(rect.x + LABEL_WIDTH, curY, rect.width - LABEL_WIDTH - 110f, FIELD_HEIGHT);
            Rect editButtonRect = new Rect(rect.x + rect.width - 100f, curY, 100f, FIELD_HEIGHT);

            Widgets.Label(labelRect, label);

            if (Widgets.ButtonText(fieldRect, subDef.moteDef?.defName ?? "Select MoteDef"))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                List<ThingDef> moteDefs = DefDatabase<ThingDef>.AllDefs
                    .Where(d => typeof(Mote).IsAssignableFrom(d.thingClass))
                    .OrderBy(d => d.defName)
                    .ToList();

                options.Add(new FloatMenuOption("None", () => {
                    subDef.moteDef = null;
                }));

                foreach (ThingDef moteDef in moteDefs)
                {
                    options.Add(new FloatMenuOption(moteDef.defName, () => {
                        subDef.moteDef = moteDef;
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            GUI.enabled = subDef.moteDef != null;
            if (Widgets.ButtonText(editButtonRect, "Edit Mote"))
            {
                Find.WindowStack.Add(new MoteEditorWindow(subDef.moteDef));
            }
            GUI.enabled = true;

            return curY + FIELD_HEIGHT + 5f;
        }

        private EffecterDef DeepCopyEffecterDef(EffecterDef source)
        {
            EffecterDef copy = new EffecterDef();

            copy.defName = source.defName;
            copy.label = source.label;
            copy.description = source.description;
            copy.positionRadius = source.positionRadius;
            copy.offsetTowardsTarget = new FloatRange(source.offsetTowardsTarget.min, source.offsetTowardsTarget.max);
            copy.maintainTicks = source.maintainTicks;
            copy.randomWeight = source.randomWeight;
            copy.offsetMode = source.offsetMode;

            if (source.children != null)
            {
                copy.children = new List<SubEffecterDef>();

                foreach (SubEffecterDef sourceSubDef in source.children)
                {
                    SubEffecterDef subDefCopy = DeepCopySubEffecterDef(sourceSubDef);
                    copy.children.Add(subDefCopy);
                }
            }

            return copy;
        }

        private SubEffecterDef DeepCopySubEffecterDef(SubEffecterDef source)
        {
            SubEffecterDef copy = new SubEffecterDef();

            copy.subEffecterClass = source.subEffecterClass;
            copy.burstCount = new IntRange(source.burstCount.min, source.burstCount.max);
            copy.ticksBetweenMotes = source.ticksBetweenMotes;
            copy.maxMoteCount = source.maxMoteCount;
            copy.initialDelayTicks = source.initialDelayTicks;
            copy.lifespanMaxTicks = source.lifespanMaxTicks;
            copy.chancePerTick = source.chancePerTick;
            copy.chancePeriodTicks = source.chancePeriodTicks;
            copy.spawnLocType = source.spawnLocType;
            copy.positionLerpFactor = source.positionLerpFactor;
            copy.positionOffset = source.positionOffset;
            copy.positionRadius = source.positionRadius;
            copy.positionRadiusMin = source.positionRadiusMin;

            if (source.perRotationOffsets != null)
            {
                copy.perRotationOffsets = new List<Vector3>(source.perRotationOffsets);
            }

            copy.positionDimensions = source.positionDimensions;
            copy.attachToSpawnThing = source.attachToSpawnThing;
            copy.avoidLastPositionRadius = source.avoidLastPositionRadius;
            copy.attachPoint = source.attachPoint;
            copy.moteDef = source.moteDef;
            copy.fleckDef = source.fleckDef;
            copy.color = source.color;
            copy.angle = new FloatRange(source.angle.min, source.angle.max);
            copy.absoluteAngle = source.absoluteAngle;
            copy.useTargetAInitialRotation = source.useTargetAInitialRotation;
            copy.useTargetBInitialRotation = source.useTargetBInitialRotation;
            copy.fleckUsesAngleForVelocity = source.fleckUsesAngleForVelocity;
            copy.rotateTowardsTargetCenter = source.rotateTowardsTargetCenter;
            copy.useTargetABodyAngle = source.useTargetABodyAngle;
            copy.useTargetBBodyAngle = source.useTargetBBodyAngle;

            if (source.speed != null)
                copy.speed = new FloatRange(source.speed.min, source.speed.max);
            if (source.rotation != null)
                copy.rotation = new FloatRange(source.rotation.min, source.rotation.max);
            if (source.rotationRate != null)
                copy.rotationRate = new FloatRange(source.rotationRate.min, source.rotationRate.max);
            if (source.scale != null)
                copy.scale = new FloatRange(source.scale.min, source.scale.max);
            if (source.airTime != null)
                copy.airTime = new FloatRange(source.airTime.min, source.airTime.max);

            copy.soundDef = source.soundDef;

            if (source.intermittentSoundInterval != null)
                copy.intermittentSoundInterval = new IntRange(source.intermittentSoundInterval.min, source.intermittentSoundInterval.max);

            copy.ticksBeforeSustainerStart = source.ticksBeforeSustainerStart;
            copy.orbitOrigin = source.orbitOrigin;

            if (source.orbitSpeed != null)
                copy.orbitSpeed = new FloatRange(source.orbitSpeed.min, source.orbitSpeed.max);

            copy.orbitSnapStrength = source.orbitSnapStrength;
            copy.makeMoteOnSubtrigger = source.makeMoteOnSubtrigger;
            copy.destroyMoteOnCleanup = source.destroyMoteOnCleanup;

            if (source.cameraShake != null)
                copy.cameraShake = new FloatRange(source.cameraShake.min, source.cameraShake.max);

            copy.distanceAttenuationScale = source.distanceAttenuationScale;
            copy.distanceAttenuationMax = source.distanceAttenuationMax;
            copy.randomWeight = source.randomWeight;
            copy.subTriggerOnSpawn = source.subTriggerOnSpawn;


            if (source.children != null)
            {
                copy.children = new List<SubEffecterDef>();

                foreach (SubEffecterDef childSubDef in source.children)
                {
                    copy.children.Add(DeepCopySubEffecterDef(childSubDef));
                }
            }

            return copy;
        }

        public string EffecterDefToXmlString(EffecterDef effecterDef)
        {
            XmlDocument doc = new XmlDocument();

            // Create root element
            XmlElement rootElement = doc.CreateElement("EffecterDef");
            doc.AppendChild(rootElement);

            if (!effecterDef.defName.NullOrEmpty())
            {
                XmlElement defNameElement = doc.CreateElement("defName");
                defNameElement.InnerText = effecterDef.defName;
                rootElement.AppendChild(defNameElement);
            }

            if (!effecterDef.label.NullOrEmpty())
            {
                XmlElement labelElement = doc.CreateElement("label");
                labelElement.InnerText = effecterDef.label;
                rootElement.AppendChild(labelElement);
            }

            if (!effecterDef.description.NullOrEmpty())
            {
                XmlElement descElement = doc.CreateElement("description");
                descElement.InnerText = effecterDef.description;
                rootElement.AppendChild(descElement);
            }

            if (effecterDef.positionRadius != 0f)
            {
                XmlElement radiusElement = doc.CreateElement("positionRadius");
                radiusElement.InnerText = effecterDef.positionRadius.ToString("0.##");
                rootElement.AppendChild(radiusElement);
            }

            if (effecterDef.offsetTowardsTarget.min != 0f || effecterDef.offsetTowardsTarget.max != 0f)
            {
                XmlElement offsetElement = doc.CreateElement("offsetTowardsTarget");
                offsetElement.InnerText = FloatRangeToString(effecterDef.offsetTowardsTarget);
                rootElement.AppendChild(offsetElement);
            }

            if (effecterDef.maintainTicks != 0)
            {
                XmlElement ticksElement = doc.CreateElement("maintainTicks");
                ticksElement.InnerText = effecterDef.maintainTicks.ToString();
                rootElement.AppendChild(ticksElement);
            }

            if (effecterDef.randomWeight != 0f)
            {
                XmlElement weightElement = doc.CreateElement("randomWeight");
                weightElement.InnerText = effecterDef.randomWeight.ToString("0.##");
                rootElement.AppendChild(weightElement);
            }


            if (effecterDef.offsetMode != EffecterDef.OffsetMode.TrueCenterCompensated)
            {
                XmlElement modeElement = doc.CreateElement("offsetMode");
                modeElement.InnerText = effecterDef.offsetMode.ToString();
                rootElement.AppendChild(modeElement);
            }


            if (effecterDef.children != null && effecterDef.children.Count > 0)
            {
                XmlElement childrenElement = doc.CreateElement("children");
                rootElement.AppendChild(childrenElement);

                foreach (SubEffecterDef subDef in effecterDef.children)
                {
                    XmlElement liElement = doc.CreateElement("li");
                    childrenElement.AppendChild(liElement);
                    AddSubEffecterDefToXml(doc, liElement, subDef);
                }
            }

            return FormatXml(doc);
        }

        private void AddSubEffecterDefToXml(XmlDocument doc, XmlElement parent, SubEffecterDef subDef)
        {
            if (subDef.subEffecterClass != null)
            {
                XmlElement classElement = doc.CreateElement("subEffecterClass");
                classElement.InnerText = subDef.subEffecterClass.Name;
                parent.AppendChild(classElement);
            }

            if (subDef.burstCount.min != 1 || subDef.burstCount.max != 1)
            {
                XmlElement burstElement = doc.CreateElement("burstCount");
                burstElement.InnerText = IntRangeToString(subDef.burstCount);
                parent.AppendChild(burstElement);
            }

            if (subDef.ticksBetweenMotes != 0)
            {
                XmlElement ticksElement = doc.CreateElement("ticksBetweenMotes");
                ticksElement.InnerText = subDef.ticksBetweenMotes.ToString();
                parent.AppendChild(ticksElement);
            }


            if (subDef.maxMoteCount != 0)
            {
                XmlElement maxElement = doc.CreateElement("maxMoteCount");
                maxElement.InnerText = subDef.maxMoteCount.ToString();
                parent.AppendChild(maxElement);
            }


            if (subDef.initialDelayTicks != 0)
            {
                XmlElement delayElement = doc.CreateElement("initialDelayTicks");
                delayElement.InnerText = subDef.initialDelayTicks.ToString();
                parent.AppendChild(delayElement);
            }

            if (subDef.lifespanMaxTicks != 0)
            {
                XmlElement lifespanElement = doc.CreateElement("lifespanMaxTicks");
                lifespanElement.InnerText = subDef.lifespanMaxTicks.ToString();
                parent.AppendChild(lifespanElement);
            }

            if (subDef.spawnLocType != MoteSpawnLocType.OnSource)
            {
                XmlElement locTypeElement = doc.CreateElement("spawnLocType");
                locTypeElement.InnerText = subDef.spawnLocType.ToString();
                parent.AppendChild(locTypeElement);
            }

            if (subDef.positionOffset != Vector3.zero)
            {
                XmlElement offsetElement = doc.CreateElement("positionOffset");
                offsetElement.InnerText = Vector3ToString(subDef.positionOffset);
                parent.AppendChild(offsetElement);
            }

            if (subDef.positionRadius != 0f)
            {
                XmlElement radiusElement = doc.CreateElement("positionRadius");
                radiusElement.InnerText = subDef.positionRadius.ToString("0.##");
                parent.AppendChild(radiusElement);
            }

            if (subDef.positionRadiusMin != 0f)
            {
                XmlElement radiusMinElement = doc.CreateElement("positionRadiusMin");
                radiusMinElement.InnerText = subDef.positionRadiusMin.ToString("0.##");
                parent.AppendChild(radiusMinElement);
            }

            if (subDef.moteDef != null)
            {
                XmlElement moteElement = doc.CreateElement("moteDef");
                moteElement.InnerText = subDef.moteDef.defName;
                parent.AppendChild(moteElement);
            }

            if (subDef.fleckDef != null)
            {
                XmlElement fleckElement = doc.CreateElement("fleckDef");
                fleckElement.InnerText = subDef.fleckDef.defName;
                parent.AppendChild(fleckElement);
            }

            if (subDef.color != Color.white)
            {
                XmlElement colorElement = doc.CreateElement("color");
                colorElement.InnerText = ColorToString(subDef.color);
                parent.AppendChild(colorElement);
            }

            if (subDef.angle.min != 0f || subDef.angle.max != 0f)
            {
                XmlElement angleElement = doc.CreateElement("angle");
                angleElement.InnerText = FloatRangeToString(subDef.angle);
                parent.AppendChild(angleElement);
            }

            if (subDef.absoluteAngle)
            {
                XmlElement absAngleElement = doc.CreateElement("absoluteAngle");
                absAngleElement.InnerText = "true";
                parent.AppendChild(absAngleElement);
            }

            if (subDef.speed != null)
            {
                XmlElement speedElement = doc.CreateElement("speed");
                speedElement.InnerText = FloatRangeToString(subDef.speed);
                parent.AppendChild(speedElement);
            }


            if (subDef.rotation != null)
            {
                XmlElement rotationElement = doc.CreateElement("rotation");
                rotationElement.InnerText = FloatRangeToString(subDef.rotation);
                parent.AppendChild(rotationElement);
            }


            if (subDef.rotationRate != null)
            {
                XmlElement rotRateElement = doc.CreateElement("rotationRate");
                rotRateElement.InnerText = FloatRangeToString(subDef.rotationRate);
                parent.AppendChild(rotRateElement);
            }


            if (subDef.scale != null)
            {
                XmlElement scaleElement = doc.CreateElement("scale");
                scaleElement.InnerText = FloatRangeToString(subDef.scale);
                parent.AppendChild(scaleElement);
            }


            if (subDef.airTime != null)
            {
                XmlElement airTimeElement = doc.CreateElement("airTime");
                airTimeElement.InnerText = FloatRangeToString(subDef.airTime);
                parent.AppendChild(airTimeElement);
            }


            if (subDef.soundDef != null)
            {
                XmlElement soundElement = doc.CreateElement("soundDef");
                soundElement.InnerText = subDef.soundDef.defName;
                parent.AppendChild(soundElement);
            }

            if (subDef.orbitSnapStrength != 0)
            {
                XmlElement orbitSnapStrength = doc.CreateElement("orbitSnapStrength");
                orbitSnapStrength.InnerText = subDef.orbitSnapStrength.ToString();
                parent.AppendChild(orbitSnapStrength);
            }

            if (subDef.orbitSpeed != default(FloatRange))
            {
                XmlElement orbitSpeed = doc.CreateElement("orbitSpeed");
                orbitSpeed.InnerText = subDef.orbitSpeed.ToString();
                parent.AppendChild(orbitSpeed);
            }


            AddBooleanProperty(doc, parent, "attachToSpawnThing", subDef.attachToSpawnThing);
            AddBooleanProperty(doc, parent, "useTargetAInitialRotation", subDef.useTargetAInitialRotation);
            AddBooleanProperty(doc, parent, "useTargetBInitialRotation", subDef.useTargetBInitialRotation);
            AddBooleanProperty(doc, parent, "fleckUsesAngleForVelocity", subDef.fleckUsesAngleForVelocity);
            AddBooleanProperty(doc, parent, "rotateTowardsTargetCenter", subDef.rotateTowardsTargetCenter);
            AddBooleanProperty(doc, parent, "useTargetABodyAngle", subDef.useTargetABodyAngle);
            AddBooleanProperty(doc, parent, "useTargetBBodyAngle", subDef.useTargetBBodyAngle);
            AddBooleanProperty(doc, parent, "orbitOrigin", subDef.orbitOrigin);

            AddBooleanProperty(doc, parent, "makeMoteOnSubtrigger", subDef.makeMoteOnSubtrigger);
            AddBooleanProperty(doc, parent, "destroyMoteOnCleanup", subDef.destroyMoteOnCleanup);
            AddBooleanProperty(doc, parent, "subTriggerOnSpawn", subDef.subTriggerOnSpawn);

            if (subDef.children != null && subDef.children.Count > 0)
            {
                XmlElement childrenElement = doc.CreateElement("children");
                parent.AppendChild(childrenElement);

                foreach (SubEffecterDef childSubDef in subDef.children)
                {
                    XmlElement liElement = doc.CreateElement("li");
                    childrenElement.AppendChild(liElement);
                    AddSubEffecterDefToXml(doc, liElement, childSubDef);
                }
            }
        }

        private void AddBooleanProperty(XmlDocument doc, XmlElement parent, string name, bool value)
        {
            if (value)
            {
                XmlElement element = doc.CreateElement(name);
                element.InnerText = "true";
                parent.AppendChild(element);
            }
        }

        private string FloatRangeToString(FloatRange range)
        {
            if (range.min == range.max)
            {
                return range.min.ToString("0.##");
            }
            return $"{range.min.ToString("0.##")}~{range.max.ToString("0.##")}";
        }

        private string IntRangeToString(IntRange range)
        {
            if (range.min == range.max)
            {
                return range.min.ToString();
            }
            return $"{range.min}~{range.max}";
        }

        private string Vector3ToString(Vector3 vec)
        {
            return $"({vec.x.ToString("0.##")},{vec.y.ToString("0.##")},{vec.z.ToString("0.##")})";
        }

        private string ColorToString(Color color)
        {
            return $"({color.r.ToString("0.###")},{color.g.ToString("0.###")},{color.b.ToString("0.###")},{color.a.ToString("0.###")})";
        }

        public static string FormatXml(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace
            };

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }

            return sb.ToString();
        }
    }
}
