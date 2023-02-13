﻿using UnityEngine.SceneManagement;

namespace WellRoundedBalance.Mechanic.CommencementSpeedBoost
{
    internal class CommencementSpeedBoost : MechanicBase
    {
        public static BuffDef commencementSpeed;
        public override string Name => ":: Mechanics ::::::::: Commencement Speed Boost";

        public override void Init()
        {
            var genericSpeed = Utils.Paths.Texture2D.texMovespeedBuffIcon.Load<Texture2D>();

            commencementSpeed = ScriptableObject.CreateInstance<BuffDef>();
            commencementSpeed.isHidden = false;
            commencementSpeed.buffColor = new Color32(191, 221, 255, 225);
            commencementSpeed.iconSprite = Sprite.Create(genericSpeed, new Rect(0, 0, (float)genericSpeed.width, (float)genericSpeed.height), new Vector2(0f, 0f));
            commencementSpeed.canStack = false;
            commencementSpeed.isDebuff = false;
            commencementSpeed.name = "Commencement Speed Boost";

            ContentAddition.AddBuffDef(commencementSpeed);

            base.Init();
        }

        public override void Hooks()
        {
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody characterBody)
        {
            if (SceneManager.GetActiveScene().name == "moon2")
            {
                var commencementSpeedBuffController = characterBody.GetComponent<CommencementSpeedBuffController>();
                if (characterBody.isPlayerControlled && commencementSpeedBuffController == null)
                {
                    characterBody.gameObject.AddComponent<CommencementSpeedBuffController>();
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(commencementSpeed))
            {
                args.moveSpeedMultAdd += 0.75f;
            }
        }
    }

    public class CommencementSpeedBuffController : MonoBehaviour
    {
        private bool wasGiven = false;
        private CharacterBody characterBody;

        private void Start()
        {
            characterBody = GetComponent<CharacterBody>();
            if (!wasGiven)
            {
                characterBody.AddTimedBuff(CommencementSpeedBoost.commencementSpeed, 45f);
                wasGiven = true;
            }
        }
    }
}