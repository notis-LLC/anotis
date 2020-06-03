using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShikimoriSharp;
using ShikimoriSharp.Bases;
using ShikimoriSharp.Classes;
using ShikimoriSharp.Settings;

namespace Anotis.Models.SourceWorkers
{
    public class ShikimoriWorker : ISourceWorker
    {
        
        private const string RedirectUri = "https://cc8b707e.ngrok.io/shikimori/auth";
        private const string ClientName = "ShikimoriSharp";
        private const string ClientId = "nsLe5UGPbFIYLeC_q3Wbm65-wOgB6JOLN46Ukmt_RQM";
        private const string ClientSecret = "lCN8nVFM2GDP8hayyLE6dYlzLYVbsE_7pK0OPC93Yk0";

        private static readonly ShikimoriClient Client =
            ShikimoriClient.Create(ClientName, ClientId, ClientSecret, new AccessToken(), RedirectUri);
        public async Task<AccessToken> GenerateToken(string code)
        {
            await Client.Client.Auth(code);
            return Client.Client.Token;
        }

        public async Task<User> GetUser(AccessToken token)
        {
            Client.Client.Auth(token);
            
            var whoami = await Client.Users.WhoAmI();
            
            var myList = await Client.UserRates.GetUsersRates(new UserRatesSettings
            {
                user_id = whoami.Id
            });

            var animes = (await Parse(myList, "Anime", Client.Animes.GetAnime)).ToList();
            var mangas = (await Parse(myList, "Manga", Client.Mangas.GetManga)).ToList();
            return new User
            {
                Animes = animes,
                Mangas = mangas,
                Token = token
            };
        }

        private static async Task<T[]> Parse<T>(IEnumerable<UserRate> rates, string query, Func<long, Task<T>> func)
        {
            return await Task.WhenAll(rates.Where(it => it.TargetType == query).Select(it => func(it.TargetId!.Value)));
        }

    }
}