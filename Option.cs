/**
 * ngetopt - Unixy command-line option parser library for .NET
 * Copyright (c) 2010 Bob Carroll
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ngetopt
{
    /// <summary>
    /// Long option argument flags.
    /// </summary>
    public enum ArgFlags : int
    {
        None = 0,
        Required = 1,
        Optional = 2
    }

    /// <summary>
    /// Long option structure.
    /// </summary>
    public struct Option
    {
        public string Name;     /* long name for this option */
        public ArgFlags HasArg; /* 0 = no argument, 1 = required argument, 2 = optional argument */
        public int Flag;        /* this is not implemented so it must always be zero */
        public char Val;        /* value to return from getopt() */
    }
}
