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
    /// Extra goodies for options structure.
    /// </summary>
    public struct OptionExtra
    {
        public Option Option;       /* reference to associated option struct */
        public string Description;  /* a brief description of the option */
        public string InGroup;      /* name of the group this option belongs to */
        public string ArgLabel;     /* label for the option argument */
    }
}