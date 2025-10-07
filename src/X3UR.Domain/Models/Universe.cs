using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X3UR.Domain.Models;
public class Universe {
    public Sector[,] Map { get; init; }
    public List<Cluster> Clusters { get; } = new(); 
}