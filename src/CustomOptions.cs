using Essentials.Options;

namespace MinatoMod
{
    public static class CustomOptions
    {
        public static MinatoSetting EnableMinato;

        public static CustomStringOption MinatoEnabledOption;

        public enum MinatoSetting
        {
            Always,
            Maybe,
            Never
        }

        public static MinatoSetting GetMinatoSetting()
        {
            return MinatoEnabledOption.GetValue<MinatoSetting>();
        }
    }
}
