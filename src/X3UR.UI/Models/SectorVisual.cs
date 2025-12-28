using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using X3UR.Domain.Enums;

namespace X3UR.UI.Models;
public class SectorVisual {
    public byte X { get; init; }
    public byte Y { get; init; }
    public RaceNames Race { get; init; }
    public Brush Fill { get; init; }
}