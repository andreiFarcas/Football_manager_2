﻿using System;
using System.Collections.Generic;

namespace ClientApplication.Models;

public partial class Standing
{
    public int StandingsId { get; set; }

    public int UserId { get; set; }

    public int Position { get; set; }

    public int MatchesPlayed { get; set; }

    public int MatchesWon { get; set; }

    public int MatchesDrawn { get; set; }

    public int MatchesLost { get; set; }

    public int GoalsFor { get; set; }

    public int GoalsAgainst { get; set; }

    public int Trophies { get; set; }

    public virtual User User { get; set; } = null!;
}
