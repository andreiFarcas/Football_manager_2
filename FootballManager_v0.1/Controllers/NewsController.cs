﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootballManager_v0._1.Models;

namespace FootballManager_v0._1.Controllers
{
    public class NewsController : Controller
    {
        private readonly FootballDatabaseContext _context;

        public NewsController(FootballDatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var footballDatabaseContext = _context.News.Include(p => p.Admin);
            return View(await footballDatabaseContext.ToListAsync());
        }

        // GET: Players/Create
        public IActionResult Create()
        {
            ViewData["League" +
                "Id"] = new SelectList(_context.Leagues, "LeagueId", "LeagueId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdminId,Post")] News news)
        {
            // Autogenerate a new id
            int maxId = _context.News.Max(n => n.NewsId);
            news.NewsId = maxId + 1;

            // Add the news to the db
            _context.Add(news);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            ViewData["AdminId"] = new SelectList(_context.Administrators, "AdminId", "AdminId", news.AdminId);
            return View(news);
        }

        // POST: News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NewsId,AdminId,Post")] News news)
        {
            if (id != news.NewsId)
            {
                return NotFound();
            }

            try
            {
                _context.Update(news);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsExists(news.NewsId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
            ViewData["AdminId"] = new SelectList(_context.Administrators, "AdminId", "AdminId", news.AdminId);
            return View(news);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.News == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .Include(p => p.Admin)
                .FirstOrDefaultAsync(m => m.NewsId == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.News == null)
            {
                return Problem("Entity set 'FootballDatabaseContext.News'  is null.");
            }
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return(_context.News?.Any(e => e.NewsId == id)).GetValueOrDefault();
        }

        public IActionResult Index_autogenerated()
        {
            return View();
        }

    }
}


