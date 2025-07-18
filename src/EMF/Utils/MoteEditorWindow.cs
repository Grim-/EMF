using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class MoteEditorWindow : Window
    {
        private ThingDef moteDef;
        private Vector2 scrollPosition = Vector2.zero;
        private float viewHeight;

        public override Vector2 InitialSize => new Vector2(800f, 600f);

        public MoteEditorWindow(ThingDef moteDef)
        {
            this.moteDef = moteDef;
            this.forcePause = true;
            this.doCloseX = true;
            this.closeOnClickedOutside = false;
            this.absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(0f, 0f, inRect.width, 40f);
            Widgets.Label(titleRect, $"Editing Mote: {moteDef.defName}");
            Text.Font = GameFont.Small;

            Rect contentRect = new Rect(0f, 45f, inRect.width, inRect.height - 45f - 35f);
            Rect viewRect = new Rect(0f, 0f, contentRect.width - 20f, viewHeight);

            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);

            float curY = 0f;
            float fieldHeight = 30f;
            float labelWidth = 200f;

            // DefName
            Rect defNameLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect defNameFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);
            Widgets.Label(defNameLabelRect, "DefName:");
            moteDef.defName = Widgets.TextField(defNameFieldRect, moteDef.defName);
            curY += fieldHeight + 5f;

            // Graphics section header
            Text.Font = GameFont.Medium;
            Rect graphicsHeaderRect = new Rect(0f, curY, viewRect.width, fieldHeight);
            Widgets.Label(graphicsHeaderRect, "Graphics");
            Text.Font = GameFont.Small;
            curY += fieldHeight + 5f;

            // Ensure GraphicData exists
            if (moteDef.graphicData == null)
            {
                moteDef.graphicData = new GraphicData();
            }

            // Texture Path
            Rect texPathLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect texPathFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);
            Widgets.Label(texPathLabelRect, "Texture Path:");
            moteDef.graphicData.texPath = Widgets.TextField(texPathFieldRect, moteDef.graphicData.texPath);
            curY += fieldHeight + 5f;

            // Graphic Class
            Rect graphicClassLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect graphicClassFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);
            Widgets.Label(graphicClassLabelRect, "Graphic Class:");

            if (Widgets.ButtonText(graphicClassFieldRect, moteDef.graphicData.graphicClass?.Name ?? "Select Graphic Class"))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach (Type type in GenTypes.AllTypes.Where(t => typeof(Graphic).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    options.Add(new FloatMenuOption(type.Name, () => {
                        moteDef.graphicData.graphicClass = type;
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            curY += fieldHeight + 5f;

            // DrawSize
            Rect drawSizeLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect drawSizeXRect = new Rect(labelWidth, curY, (viewRect.width - labelWidth) / 2 - 5f, fieldHeight);
            Rect drawSizeYRect = new Rect(labelWidth + (viewRect.width - labelWidth) / 2 + 5f, curY, (viewRect.width - labelWidth) / 2 - 5f, fieldHeight);

            Widgets.Label(drawSizeLabelRect, "Draw Size (X/Y):");

            string xBuffer = moteDef.graphicData.drawSize.x.ToString();
            float x = moteDef.graphicData.drawSize.x;
            Widgets.TextFieldNumeric(drawSizeXRect, ref x, ref xBuffer);

            string yBuffer = moteDef.graphicData.drawSize.y.ToString();
            float y = moteDef.graphicData.drawSize.y;
            Widgets.TextFieldNumeric(drawSizeYRect, ref y, ref yBuffer);

            moteDef.graphicData.drawSize = new Vector2(x, y);
            curY += fieldHeight + 5f;

            // Color
            Rect colorLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect colorFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(colorLabelRect, "Color:");
            moteDef.graphicData.color = ColorWidgets.ColorField(colorFieldRect, moteDef.graphicData.color);

            curY += fieldHeight + 5f;

            // MoteProperties section header
            Text.Font = GameFont.Medium;
            Rect motePropsHeaderRect = new Rect(0f, curY, viewRect.width, fieldHeight);
            Widgets.Label(motePropsHeaderRect, "Mote Properties");
            Text.Font = GameFont.Small;
            curY += fieldHeight + 5f;

            // Ensure MoteProperties exists
            if (moteDef.mote == null)
            {
                moteDef.mote = new MoteProperties();
            }

            // Real Time
            Rect realTimeLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect realTimeCheckRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(realTimeLabelRect, "Real Time:");
            Widgets.Checkbox(labelWidth, curY + (fieldHeight / 2) - 12f, ref moteDef.mote.realTime);
            curY += fieldHeight + 5f;

            // Fade Times
            Rect fadeInLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect fadeInFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(fadeInLabelRect, "Fade In Time:");
            string fadeInBuffer = moteDef.mote.fadeInTime.ToString();
            Widgets.TextFieldNumeric(fadeInFieldRect, ref moteDef.mote.fadeInTime, ref fadeInBuffer);
            curY += fieldHeight + 5f;

            Rect solidTimeLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect solidTimeFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(solidTimeLabelRect, "Solid Time:");
            string solidTimeBuffer = moteDef.mote.solidTime.ToString();
            Widgets.TextFieldNumeric(solidTimeFieldRect, ref moteDef.mote.solidTime, ref solidTimeBuffer);
            curY += fieldHeight + 5f;

            Rect fadeOutLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect fadeOutFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(fadeOutLabelRect, "Fade Out Time:");
            string fadeOutBuffer = moteDef.mote.fadeOutTime.ToString();
            Widgets.TextFieldNumeric(fadeOutFieldRect, ref moteDef.mote.fadeOutTime, ref fadeOutBuffer);
            curY += fieldHeight + 5f;

            // Acceleration
            Rect accelLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect accelXRect = new Rect(labelWidth, curY, (viewRect.width - labelWidth) / 3 - 5f, fieldHeight);
            Rect accelYRect = new Rect(labelWidth + (viewRect.width - labelWidth) / 3 + 5f, curY, (viewRect.width - labelWidth) / 3 - 5f, fieldHeight);
            Rect accelZRect = new Rect(labelWidth + 2 * ((viewRect.width - labelWidth) / 3 + 5f), curY, (viewRect.width - labelWidth) / 3 - 5f, fieldHeight);

            Widgets.Label(accelLabelRect, "Acceleration (X/Y/Z):");

            string accelXBuffer = moteDef.mote.acceleration.x.ToString();
            float accelX = moteDef.mote.acceleration.x;
            Widgets.TextFieldNumeric(accelXRect, ref accelX, ref accelXBuffer);

            string accelYBuffer = moteDef.mote.acceleration.y.ToString();
            float accelY = moteDef.mote.acceleration.y;
            Widgets.TextFieldNumeric(accelYRect, ref accelY, ref accelYBuffer);

            string accelZBuffer = moteDef.mote.acceleration.z.ToString();
            float accelZ = moteDef.mote.acceleration.z;
            Widgets.TextFieldNumeric(accelZRect, ref accelZ, ref accelZBuffer);

            moteDef.mote.acceleration = new Vector3(accelX, accelY, accelZ);
            curY += fieldHeight + 5f;

            // Speed Per Time
            Rect speedLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect speedFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(speedLabelRect, "Speed Per Time:");
            string speedBuffer = moteDef.mote.speedPerTime.ToString();
            Widgets.TextFieldNumeric(speedFieldRect, ref moteDef.mote.speedPerTime, ref speedBuffer);
            curY += fieldHeight + 5f;

            // Growth Rate
            Rect growthLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect growthFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(growthLabelRect, "Growth Rate:");
            string growthBuffer = moteDef.mote.growthRate.ToString();
            Widgets.TextFieldNumeric(growthFieldRect, ref moteDef.mote.growthRate, ref growthBuffer);
            curY += fieldHeight + 5f;

            // Collide
            Rect collideLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect collideCheckRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(collideLabelRect, "Collide:");
            Widgets.Checkbox(labelWidth, curY + (fieldHeight / 2) - 12f, ref moteDef.mote.collide);
            curY += fieldHeight + 5f;

            // Arch properties
            Rect archHeightLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect archHeightFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(archHeightLabelRect, "Arch Height:");
            string archHeightBuffer = moteDef.mote.archHeight.ToString();
            Widgets.TextFieldNumeric(archHeightFieldRect, ref moteDef.mote.archHeight, ref archHeightBuffer);
            curY += fieldHeight + 5f;

            Rect archDurationLabelRect = new Rect(0f, curY, labelWidth, fieldHeight);
            Rect archDurationFieldRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

            Widgets.Label(archDurationLabelRect, "Arch Duration:");
            string archDurationBuffer = moteDef.mote.archDuration.ToString();
            Widgets.TextFieldNumeric(archDurationFieldRect, ref moteDef.mote.archDuration, ref archDurationBuffer);
            curY += fieldHeight + 5f;

            // Additional boolean properties
            string[] booleanProps = new string[]
            {
                "Rotate Towards Target",
                "Rotate Towards Move Direction",
                "Scale To Connect Targets",
                "Attached To Head",
                "Fade Out Unmaintained",
                "Update Offset To Match Target Rotation"
            };

            bool[] boolValues = new bool[]
            {
                moteDef.mote.rotateTowardsTarget,
                moteDef.mote.rotateTowardsMoveDirection,
                moteDef.mote.scaleToConnectTargets,
                moteDef.mote.attachedToHead,
                moteDef.mote.fadeOutUnmaintained,
                moteDef.mote.updateOffsetToMatchTargetRotation
            };

            for (int i = 0; i < booleanProps.Length; i++)
            {
                Rect labelRect = new Rect(0f, curY, labelWidth, fieldHeight);
                Rect checkRect = new Rect(labelWidth, curY, viewRect.width - labelWidth, fieldHeight);

                Widgets.Label(labelRect, booleanProps[i] + ":");
                Widgets.Checkbox(labelWidth, curY + (fieldHeight / 2) - 12f, ref boolValues[i]);
                curY += fieldHeight + 5f;
            }

            // Set boolean properties back
            moteDef.mote.rotateTowardsTarget = boolValues[0];
            moteDef.mote.rotateTowardsMoveDirection = boolValues[1];
            moteDef.mote.scaleToConnectTargets = boolValues[2];
            moteDef.mote.attachedToHead = boolValues[3];
            moteDef.mote.fadeOutUnmaintained = boolValues[4];
            moteDef.mote.updateOffsetToMatchTargetRotation = boolValues[5];

            // Add more MoteProperties fields as needed

            viewHeight = curY + 50f;
            Widgets.EndScrollView();

            // Save button
            Rect saveButtonRect = new Rect(inRect.width - 200f, inRect.height - 35f, 95f, 35f);
            Rect cancelButtonRect = new Rect(inRect.width - 100f, inRect.height - 35f, 95f, 35f);

            if (Widgets.ButtonText(saveButtonRect, "Save"))
            {
                // Changes to moteDef are already applied directly
                Messages.Message("MoteDef changes saved successfully.", MessageTypeDefOf.PositiveEvent);
                Close();
            }

            if (Widgets.ButtonText(cancelButtonRect, "Cancel"))
            {
                Close();
            }
        }
    }
}
