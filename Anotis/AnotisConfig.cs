namespace Anotis
{
    public class AnotisConfig
    {
        public ShikimoriConfig Shikimori { get; set; }

        public LiteDbConfig LiteDb { get; set; }

        public AnserServices Services { get; set; }
        

        public class ShikimoriConfig
        {
            public string AuthLinkTemplate { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string ClientName { get; set; }
            public string RedirectUrl { get; set; }
        }

        public class LiteDbConfig
        {
            public string ConnectionString { get; set; }
        }

        public class AnserServices
        {
            public string Manser { get; set; }
            public Tanser Tanser { get; set; }
        }

        public class Tanser
        {
            public string Send { get; set; }
            public string Link { get; set; }
        }
    }
}