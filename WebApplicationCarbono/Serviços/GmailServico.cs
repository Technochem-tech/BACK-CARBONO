using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MimeKit;
using System;
using System.IO;

namespace WebApplicationCarbono.Serviços
{
    public class GmailServico
    {
        private static readonly string ClientId = Environment.GetEnvironmentVariable("GMAIL_CLIENT_ID");
        private static readonly string ClientSecret = Environment.GetEnvironmentVariable("GMAIL_CLIENT_SECRET");
        private static readonly string RefreshToken = Environment.GetEnvironmentVariable("GMAIL_REFRESH_TOKEN");
        private const string ApplicationName = "EnvioEmailCarbono";

        public void EnviarEmail(string destinatario, string assunto, string corpo)
        {
            var credential = new UserCredential(
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = ClientId,
                        ClientSecret = ClientSecret
                    }
                }),
                "user",
                new TokenResponse
                {
                    RefreshToken = RefreshToken
                }
            );

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress("Sistema", "suporteenvioemail00000@gmail.com"));
            mensagem.To.Add(new MailboxAddress("", destinatario));
            mensagem.Subject = assunto;
            mensagem.Body = new TextPart("html") { Text = corpo };

            using (var memory = new MemoryStream())
            {
                mensagem.WriteTo(memory);
                var raw = Convert.ToBase64String(memory.ToArray())
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace("=", "");

                var msg = new Message { Raw = raw };
                service.Users.Messages.Send(msg, "me").Execute();
            }
        }
    }
}