using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X3UR.Domain.DTOs;
using X3UR.Domain.Enums;

namespace X3UR.Domain.Models;
public class Sector {
    private List<Sector> _freeSpaces;
    private List<Sector> _claimers;

    public byte X { get; init; }
    public byte Y { get; init; }
    public Cluster Cluster { get; private set; }
    public RaceNames Race { get; private set; }

    public void Claim(Cluster cluster, RaceNames race) {
        Cluster = cluster;
        Race = race;
    }

    public void AddFreeSector(Sector sector) {
        _freeSpaces ??= new List<Sector>();
        _freeSpaces.Add(sector);
    }

    public void RemoveFreeSpace(Sector sector) {
        _freeSpaces?.Remove(sector);
    }

    public void AddClaimer(Sector sector) {
        _claimers ??= new List<Sector>();
        _claimers.Add(sector);
    }

    public void RemoveClaimer(Sector sector) {
        _claimers?.Remove(sector);
    }

    public bool CanGrow() => _freeSpaces != null && _freeSpaces.Count > 0;
    public bool CanBeClaimed() => _claimers != null && _claimers.Count > 0;
}