<%@ WebHandler Language="C#" Class="TicketHandler" %>

using DevBoard;
using DevBoard.Models;
using DevBoard.Services;
using Newtonsoft.Json;
using System;
using System.Web;

public class TicketHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        if (!context.Request.IsAuthenticated)
        {
            context.Response.StatusCode = 401;
            context.Response.Write(JsonConvert.SerializeObject(new { success = false, message = "Unauthorized" }));
            return;
        }

        var action = context.Request.Form["action"];

        try
        {
            using (var dbContext = new DevBoardContext())
            {
                var ticketService = new TicketService(dbContext);

                if (action == "updateStatus")
                {
                    int ticketId = int.Parse(context.Request.Form["ticketId"]);
                    int statusValue = int.Parse(context.Request.Form["status"]);
                    Status status = (Status)statusValue;

                    ticketService.UpdateTicketStatus(ticketId, status);

                    context.Response.Write(JsonConvert.SerializeObject(new { success = true }));
                }
                else if (action == "vote")
                {
                    int ticketId = int.Parse(context.Request.Form["ticketId"]);
                    int value = int.Parse(context.Request.Form["value"]);
                    string userId = context.User.Identity.Name;

                    ticketService.Vote(ticketId, userId, value);

                    int newScore = ticketService.GetTicketScore(ticketId);

                    context.Response.Write(JsonConvert.SerializeObject(new { success = true, score = newScore }));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write(JsonConvert.SerializeObject(new { success = false, message = "Invalid action" }));
                }
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.Write(JsonConvert.SerializeObject(new { success = false, message = ex.Message }));
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}
