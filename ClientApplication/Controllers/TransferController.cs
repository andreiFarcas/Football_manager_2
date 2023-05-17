﻿using ClientApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClientApplication.Controllers
{
    public class TransferController : Controller
    {

        private readonly FootballDatabaseContext _context;

        public TransferController(FootballDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult BuyPlayer(int transferId)
        {
            //Get the id of the user that is currently logged in
            try
            {
                var username = HomeController.GetUserName(HttpContext);
                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                int userID = user.UserId;

                //Remove the transfer from the available transfers by attaching a buying user

                var happeningTransfer = _context.Transfers.FirstOrDefault(t => t.TransferId == transferId);
                
                happeningTransfer.BuyingUserId = userID;

                // Update the player's contract so that the player is now owned by the buying user
                var player = _context.Players.FirstOrDefault(p => p.PlayerId == happeningTransfer.PlayerId);
                var teamContract = _context.TeamContracts.FirstOrDefault(tc => tc.PlayerId == player.PlayerId);

                teamContract.SquadId = userID;
                teamContract.Position = 19; // 19 is the position for a player that does not have a role in the team
                
                // Solve all money issues (selling user gets money, buying user loses money)
                var sellingUser = _context.Users.FirstOrDefault(u => u.UserId == happeningTransfer.SellingUserId);
                user.Coins -= happeningTransfer.TransferFee;
                sellingUser.Coins += happeningTransfer.TransferFee;

                // Updates coins viewbag fo coin display
                ViewBag.wallet = user.Coins;

                _context.SaveChanges();
            }
            catch (NullReferenceException)
            {
                return RedirectToAction("Login", "Access"); // Should only happen if the user is not logged in
            } 
            return View();
        }

        [HttpGet]
        public IActionResult TransferList(string searchString)
        {
            //Perform database query to retrieve all unsold Transfers from transfer table

            // All unsold transfers
            var transfers = _context.Transfers
                .Include(t => t.Player)
                    .ThenInclude(p => p.TeamContracts)
                .Include(t => t.SellingUser)
                .Where(t => t.BuyingUserId == null)
                .ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                // Update: Now include only players that have searchString in their name
                transfers = _context.Transfers
                    .Include(t => t.Player)
                        .ThenInclude(p => p.TeamContracts)
                    .Include(t => t.SellingUser)
                    .Where(t => t.BuyingUserId == null && (t.Player.FirstName.Contains(searchString) || t.Player.LastName.Contains(searchString)))
                    .ToList();
            }

            // Add nr of coins to the wallet
            ViewBag.wallet = getMoney();

            return View(transfers);
        }

        [HttpGet]
        // Gets players that can be added to the transfer list
        public IActionResult SellPlayer()
        {
            //Get the id of the user that is currently logged in
            try
            {
                var username = HomeController.GetUserName(HttpContext);
                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                int userID = user.UserId;

                //Get all players that are owned by the user
                var players = _context.Players
                    .Include(p => p.TeamContracts)
                    .ThenInclude(tc => tc.Squad)
                    .Where(p => p.TeamContracts.Any(tc => tc.SquadId == userID));
                
                // Get the players that are not in the first team
                List<Player> playersNotInFirstTeam = players.Where(p => p.TeamContracts.Any(tc => tc.Position == 19 && tc.SquadId == userID)).ToList();

                ViewData["Players"] = new SelectList(playersNotInFirstTeam, "PlayerId", "Name");

                return View(playersNotInFirstTeam);
            }
            catch (NullReferenceException)
            {
                return RedirectToAction("Login", "Access"); // Should only happen if the user is not logged in
            }
        }

        [HttpPost]
        // Puts a player on the transfer list
        public IActionResult ListPlayer(Transfer transfer)
        {
            //Get the id of the user that is currently logged in
            try
            {
                var username = HomeController.GetUserName(HttpContext);
                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                int userID = user.UserId;
                
                //Add the transfer to the transfer table
                transfer.SellingUserId = userID;
                transfer.SellingUserId = userID;

                // Generate a new transfer id
                var transferIds = _context.Transfers.Select(t => t.TransferId).ToList();
                int newTransferId = 0;
                foreach (int id in transferIds)
                {
                    if (id > newTransferId)
                    {
                        newTransferId = id;
                    }
                }

                transfer.TransferId = newTransferId + 1;

                _context.Transfers.Add(transfer);

                // Update the player's contract so that the player is now on the transfer list (position 20)
                var player = _context.Players.FirstOrDefault(p => p.PlayerId == transfer.PlayerId);
                var teamContract = _context.TeamContracts.FirstOrDefault(tc => tc.PlayerId == player.PlayerId);
                if (teamContract != null)
                {
                    teamContract.Position = 20;
                }

                _context.SaveChanges();
            }
            catch (NullReferenceException)
            {
                return RedirectToAction("Login", "Access"); // Should only happen if the user is not logged in
            }
            return RedirectToAction("TransferList");
        }

        public int getMoney()
        {
            var userName = HomeController.GetUserName(HttpContext);
            var user = _context.Users.FirstOrDefault(u => u.Username == userName);

            return user.Coins;
        }
    }
}