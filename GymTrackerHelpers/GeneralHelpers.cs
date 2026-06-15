using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerHelpers
{
    public static class GeneralHelpers
    {
        public static string GetWidth(string? text)
        {
            int chars = text?.Length ?? 0;

            return $"width:{Math.Max(chars * 12, 250)}px;";

        }
    }
}