using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X3UR.Domain.Models;
public class Sector {
    public short Id { get; init; }
    public byte X { get; init; }
    public byte Y { get; init; }
    public byte RaceId { get; init; }
    public bool IsCore { get; init; }
    public int Size { get; init; }
    public int MusicId { get; init; }
    public int Population { get; init; }
    public int QTrade { get; init; }
    public int QFight { get; init; }
    public int QBuild { get; init; }
    public int QThink { get; init; }

    public Sector(byte posX, byte posY) {
        X = posX;
        Y = posY;
    }
}