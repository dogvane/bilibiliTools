using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaiduFanyi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Tmt.V20180321;
using TencentCloud.Tmt.V20180321.Models;
using 弹幕合并.Common;

namespace QCloud
{
    /// <summary>
    /// 腾讯的翻译接口
    /// </summary>
    public class QCloudTransApi: ITransApi
    {
        private string secretId;
        private string secretKey;

        static internal string s_secretId;
        static internal string s_secretKey;

        private Credential cred;
        private TmtClient client;

        internal QCloudTransApi()
            : this(s_secretId, s_secretKey)
        {

        }

        public QCloudTransApi(string secretId, string secretKey)
        {
            this.secretId = secretId;
            this.secretKey = secretKey;

            cred = new Credential
            {
                SecretId = s_secretId,
                SecretKey = s_secretKey,
            };

            ClientProfile clientProfile = new ClientProfile();
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = ("tmt.tencentcloudapi.com");
            clientProfile.HttpProfile = httpProfile;

            client = new TmtClient(cred, "ap-shanghai", clientProfile);
        }

        public string GetTrans(string query, string from, string to)
        {
            try
            {
                var req = new TextTranslateRequest()
                {
                    SourceText = query,
                    Source = from,
                    Target = to,
                    ProjectId = 0,
                };

                var resp = client.TextTranslate(req).
                    ConfigureAwait(false).GetAwaiter().GetResult();
                return resp.TargetText;
            }
            catch (Exception e)
            {
                Logger.Error($"QCloud get trans fial. query:{query} from:{from} to:{to}", e);
                return string.Empty;
            }
        }
    }

    public static class QCloudTransExtend
    {
        public static void UseQCloudTransApi(this IApplicationBuilder app, IConfiguration config)
        {
            var id = config["qcloud:secretId"];
            var key = config["qcloud:secretKey"];

            //Logger.Info($"qcloud:secretId:{id}");
            //Logger.Info($"qcloud:secretKey:{key}");

            QCloudTransApi.s_secretId = id;
            QCloudTransApi.s_secretKey = key;

            //var ret = new QCloudTransApi().GetTrans("how are you. are you ok?", "en", "zh");
            //Logger.Info($"trans ret:{ret}");
        }
    }
}
