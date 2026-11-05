using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio.Alegra
{
    public static class AlegraAllocation
    {
        // Distribuye valores brutos en pesos enteros: los impuestos se desglosan despues segun cada producto.
        public static decimal[] AllocateGross(IList<decimal> weights, decimal target)
        {
            if (weights == null || weights.Count == 0) throw new ArgumentException("No hay productos para distribuir.");
            if (target < 0 || target != decimal.Truncate(target)) throw new ArgumentException("El total debe ser un valor entero no negativo en COP.");
            if (weights.Any(value => value < 0)) throw new ArgumentException("Los valores de productos no pueden ser negativos.");
            decimal sum = weights.Sum();
            if (sum <= 0) throw new ArgumentException("La base de distribución debe ser mayor que cero.");
            decimal[] exact = weights.Select(value => value / sum * target).ToArray();
            decimal[] amounts = exact.Select(decimal.Floor).ToArray();
            int remainder = checked((int)(target - amounts.Sum()));
            foreach (int index in Enumerable.Range(0, weights.Count).OrderByDescending(i => exact[i] - amounts[i]).ThenBy(i => i).Take(remainder))
                amounts[index]++;
            return amounts;
        }
    }
}
