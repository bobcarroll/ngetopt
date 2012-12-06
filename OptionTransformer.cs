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
    /// Transforms an options array into inputs valid for GlibcOpt.
    /// </summary>
    public sealed class OptionTransformer
    {
        private OptionExtra[] _opts;

        /// <summary>
        /// Initialises the options transformer.
        /// </summary>
        /// <param name="opts">an array of command-line options</param>
        public OptionTransformer(OptionExtra[] opts)
        {
            _opts = opts;
        }

        /// <summary>
        /// Converts the OptionsExtra array to an Option array.
        /// </summary>
        /// <returns>an array of Option structs</returns>
        public Option[] ToLongOpts()
        {
            return _opts.Select(i => i.Option)
                .Where(i => i.Name.Length > 1).ToArray();
        }

        /// <summary>
        /// Generates a getopt() compatible options string.
        /// </summary>
        /// <returns></returns>
        public string ToOptString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (OptionExtra oe in _opts) {
                sb.Append(oe.Option.Val);

                if (oe.Option.HasArg == ArgFlags.Required)
                    sb.Append(":");
                else if (oe.Option.HasArg == ArgFlags.Optional)
                    sb.Append("::");
            }

            return sb.ToString();
        }
    }
}