namespace FeatureFlagDemo.Models
{
    public class MultiUserSettings
    {
        public string UserName { get; set; }

        public string Audience { get; set; }

        public AboutBoxSettings AboutBoxSettings;
    }
}
