﻿using AcsEmulatorAPI.Models;

namespace AcsEmulatorAPI
{
	// https://github.com/Azure/azure-rest-api-specs/blob/main/specification/communication/data-plane/Chat/stable/2021-09-07/communicationserviceschat.json
	public static class Chat
	{
		public static void AddChatEndpoints(this WebApplication app)
		{
			app.MapPost("/chat/threads", async (HttpContext context, AcsDbContext db, CreateChatThreadRequest req) =>
			{
				var t = ChatThread.CreateNew(req.Topic);

				await db.ChatThreads.AddAsync(t);
				await db.SaveChangesAsync();

				var result = new CreateChatThreadResult
				{
					ChatThread = new ChatThreadProperties
					{
						Id = t.Id,
						Topic = t.Topic,
						CreatedOn = t.CreatedOn,
						CreatedByCommunicationIdentifier = new CommunicationIdentifier
						{
							RawId = GetRawId(context, app.Configuration)
						}
					}
				};

				return Results.Created($"/chat/threads/{t.Id}", result);
			});
		}

		private static string GetRawId(HttpContext context, IConfiguration config)
		{
			var token = context.Request.Headers.Authorization.First().Split("Bearer ")[1];
			var parsedToken = UserToken.ValidateJwtToken(config["JwtSigningKey"], token);
			return parsedToken.skypeid;
		}
	}
}