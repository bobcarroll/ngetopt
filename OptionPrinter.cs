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
    /// Prints command-line options to stdout.
    /// </summary>
    public sealed class OptionPrinter
    {
        private string _usage;
        private OptionExtra[] _opts;
        private string[] _groups;

        /// <summary>
        /// Initialises the option printer.
        /// </summary>
        /// <param name="usage">command format</param>
        /// <param name="opts">an array of options</param>
        public OptionPrinter(string usage, OptionExtra[] opts)
            : this(usage, opts, new string[] { }) { }

        /// <summary>
        /// Initialises the option printer.
        /// </summary>
        /// <param name="usage">command format</param>
        /// <param name="opts">an array of options</param>
        /// <param name="groups">an array of option group names to print</param>
        public OptionPrinter(string usage, OptionExtra[] opts, string[] groups)
        {
            _usage = usage;
            _opts = opts;
            _groups = groups;

            this.OptionWidth = 32;
        }

        /// <summary>
        /// Prints an option group to stdout.
        /// </summary>
        /// <param name="groupname">group title</param>
        public void PrintGroup(string groupname)
        {
            OptionExtra[] opts = groupname != null ?
                _opts.Where(i => i.InGroup == groupname).ToArray() :
                _opts;
            if (opts.Length == 0) return;

            StringBuilder sbgroup = new StringBuilder();
            sbgroup.AppendFormat("{0}:\n",
                groupname != null ? groupname : "Options");

            foreach (OptionExtra oe in opts) {
                StringBuilder sbline = new StringBuilder();
                sbline.AppendFormat("  -{0}", oe.Option.Val);

                string arglabel = oe.ArgLabel != null ? oe.ArgLabel : "ARG";

                if (oe.Option.HasArg == ArgFlags.Required)
                    sbline.AppendFormat(" {0}", arglabel);
                else if (oe.Option.HasArg == ArgFlags.Optional)
                    sbline.AppendFormat(" [{0}]", arglabel);

                if (oe.Option.Name.Length > 1) {
                    sbline.AppendFormat(", --{0}", oe.Option.Name);

                    if (oe.Option.HasArg == ArgFlags.Required)
                        sbline.AppendFormat("={0}", arglabel);
                    else if (oe.Option.HasArg == ArgFlags.Optional)
                        sbline.AppendFormat("=[{0}]", arglabel);
                }

                sbgroup.AppendFormat("{0}{1}\n",
                    sbline.ToString().PadRight(this.OptionWidth),
                    oe.Description);
            }

            Console.WriteLine(sbgroup.ToString());
        }

        /// <summary>
        /// Prints usage instructions to stdout.
        /// </summary>
        public void PrintUsage()
        {
            Console.WriteLine("Usage: {0}\n", _usage);

            if (_groups.Length > 0) {
                foreach (string g in _groups)
                    this.PrintGroup(g);
            } else
                this.PrintGroup(null);
        }

        /// <summary>
        /// Width of the options column.
        /// </summary>
        public int OptionWidth { get; set; }
    }
}