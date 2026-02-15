using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace VoiceroidDaemon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Aitalk内でShift-JISの変換を行うため
            // .Net Core標準でサポートされないエンコーディングを追加
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();
            
            // 設定ファイルを読み込む
            string setting_path = config.GetValue<string>("setting");
            if ((setting_path != null) && (0 < setting_path.Length))
                Setting.Path = setting_path;
            if (Setting.Load() == false)
            {
                // 読み出しに失敗したら設定ファイルを新規作成する
                if (Setting.Save() == false)
                {
                    // 作成にも失敗したら終了する
                    throw new IOException("設定ファイルの作成に失敗しました。");
                }
            }
            
            // Webサーバーを構成する
            var host_builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(web_builder =>
                {
                    web_builder.UseConfiguration(config);
                    web_builder.UseStartup<Startup>();
                    if (0 < Setting.System.ListeningAddress.Length)
                    {
                        // 待ち受けURLが指定されていればURLを設定する
                        web_builder.UseUrls(Setting.System.ListeningAddress);
                    }
                });

            var host = host_builder.Build();

            // Webサーバーを開始する
            host.Run();
        }
    }
}
