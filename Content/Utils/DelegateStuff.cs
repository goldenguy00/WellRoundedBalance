namespace WellRoundedBalance.Utils
{
    public class DelegateStuff
    {
        public delegate void AddBuff(CharacterBody self, BuffIndex buffType);
        public delegate void RemoveBuff(CharacterBody self, BuffIndex buffType);

        public static AddBuff addBuff;
        public static RemoveBuff removeBuff;

        public static void Init()
        {
            if (addBuff != null)
            {
                On.RoR2.CharacterBody.AddBuff_BuffIndex += (orig, self, buffType) =>
                {
                    orig(self, buffType);
                    addBuff(self, buffType);
                };
            }

            if (removeBuff != null)
            {
                On.RoR2.CharacterBody.RemoveBuff_BuffIndex += (orig, self, buffType) =>
                {
                    orig(self, buffType);
                    removeBuff(self, buffType);
                };
            }
        }
    }
}
