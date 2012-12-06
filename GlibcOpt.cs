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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ngetopt
{
    /// <summary>
    /// A .NET implementation of glibc's command-line option parser.
    /// </summary>
    /// <see cref="http://www.kernel.org/doc/man-pages/online/pages/man3/getopt.3.html"/>
    /// <remarks>
    /// Read the GETOPT(3) man page for detailed usage instructions.
    /// 
    /// GetOpt() and GetOptLong() are both functionally equivalent to getopt()
    /// getopt_long(), respectively. GNU extensions are supported with only 
    /// a few exceptions. POSIXLY_CORRECT is supported both as a property and
    /// an environmental variable.
    /// 
    /// The Option structure makes use of CLI conventions but is otherwise
    /// identical to glibc's option struct. Due to limitations of C#, the Flag
    /// member is unused and should always be zero.
    /// 
    /// getopt_long_only() is not supported. Using "-W foo" for long options is
    /// not supported.
    /// </remarks>
    /// <example>
    /// static int Main(string[] args)
    /// {
    ///     string[] argv = Environment.GetCommandLineArgs();
    ///     GlibcOpt gopt = new GlibcOpt();
    ///     int result;
    ///     
    ///     while ((result = gopt.GetOpt(ref argv, "abc:d::")) != -1) {
    ///     
    ///         switch (result) {
    ///         
    ///             case 'a':
    ///                 Console.WriteLine("foo");
    ///                 break;
    ///             
    ///             case 'b':
    ///                 Console.WriteLine("bar");
    ///                 break;
    ///             
    ///             case 'c':   /* this option requires an argument because a colon follows it in the option string */
    ///                 Console.WriteLine(gopt.OptArgument);
    ///                 break;
    ///             
    ///             case 'd':   /* this option has an optional argument because two colons follow it in the option string */
    ///                 if (gopt.OptArguments == null)
    ///                     Console.WriteLine("baz");
    ///                 else
    ///                     Console.WriteLine(gopt.OptArgument);
    ///                 break;
    ///             
    ///             default:
    ///                 Console.WriteLine("Usage: helloworld [OPTION]");
    ///                 return 0;
    ///         }
    ///     }
    ///     
    ///     return 0;
    /// }
    /// </example>
    public sealed class GlibcOpt
    {
        private int _nextchar;      /* index of the next character in the argv element */
        private string _optarg;     /* value of the option argument */
        private bool _opterr;       /* flag to suppress printing error for unknow options to stderr */
        private int _optind;        /* next argv index to read (initialised to 1) */
        private char _optopt;       /* unrecognised option character */

        /// <summary>
        /// Initialises the option parser.
        /// </summary>
        public GlibcOpt()
        {
            _nextchar = 0;
            _optarg = null;
            _opterr = true;
            _optind = 1;
            _optopt = '\0';

            if (Environment.GetEnvironmentVariable("POSIXLY_CORRECT") != null)
                this.POSIXLY_CORRECT = true;
        }

        /// <summary>
        /// Parses the argv array for short command-line options. The first element of
        /// argv must be the binary name!
        /// </summary>
        /// <param name="argv">reference to argv passed into main()</param>
        /// <param name="optstring">valid options list</param>
        /// <returns>the option character, -1 for no more options, or ? for error</returns>
        public int GetOpt(ref string[] argv, string optstring)
        {
            int longopts = 0;

            return GetOptLong(ref argv, optstring, new Option[] { }, ref longopts);
        }

        /// <summary>
        /// Parses the argv array for long command-line options. The first element of
        /// argv must be the binary name!
        /// </summary>
        /// <param name="argv">reference to argv passed into main()</param>
        /// <param name="optstring">valid options list</param>
        /// <param name="longopts">an array of valid long options</param>
        /// <param name="longindex">returns the current long option index</param>
        /// <returns>the option character, -1 for no more options, or ? for error</returns>
        /// <remarks>For long options only, optstring should be an empty string and not null!</remarks>
        public int GetOptLong(ref string[] argv, string optstring, Option[] longopts, ref int longindex)
        {
            string imgname = Path.GetFileName(argv[0]);

            if (!Regex.IsMatch(optstring, "^([+]|-|:)?([A-Za-z0-9]:?:?)*$"))
                throw new Exception("Bad option string!");

            int res;
            Option[] validopts = this.OptStr2LongOpts(optstring);

            if (_optind < 1)
                _optind = 1;

            while (_optind < argv.Length) {
                string curitm = argv[_optind];
                bool hasarg = false;

                if (curitm == "--") {   /* -- always ends option scanning */

                    /* if we're not at the end of the array, then optind points to
                     * the first non-option in the array */
                    if (_optind + 1 < argv.Length)
                        _optind++;

                    /* end of option scanning */
                    return -1;
                } else if (this.IsOption(curitm)) {
                    char curchar;
                    Option curopt;
                    bool islong = false;
                    int nextoffset = 1;

                    if (this.IsOptionLong(curitm)) {    /* long options */

                        /* strip off the leading dashes and argument if there is one */
                        string optname = curitm.IndexOf('=') > 0 ?
                            curitm.Substring(2, curitm.IndexOf('=') - 2) :
                            curitm.Substring(2);

                        curopt = longopts.Where(o => o.Name == optname).SingleOrDefault();
                        curchar = curopt.Name != null ? curopt.Val : '\0';
                        islong = true;

                        longindex = curopt.Name != null ?
                            longopts.Select((e, i) => new { Elem = e, Index = i })
                                .Where(i => ((Option)i.Elem).Equals(curopt)).First().Index :
                            0;
                    } else {                            /* short options */
                        curchar = curitm.Substring(_nextchar + 1, 1)[0];
                        curopt = validopts.Where(o => o.Name[0] == curchar).SingleOrDefault();
                    }

                    _optopt = '\0';
                    _optarg = null;

                    if (curopt.Name != null) {
                        res = curopt.Val;
                        hasarg = curopt.HasArg != ArgFlags.None;

                        /* short option args can be the remainder of the option element or the next argv element
                         * ex: -ofoo    where -o is the option and 'foo' is the argument
                         * ex: -o foo
                         *
                         * long option args can be part of the option element if separated by equals, or the next argv element
                         * ex: --output=foo     where --output is option and 'foo' is the argument
                         * ex: --output foo
                         */
                        if (!islong && hasarg && curitm.Length > 2 && _nextchar == 0) {
                            _optarg = curitm.Substring(2);
                        } else if (!islong && hasarg && curitm.Length == 2 && _optind + 1 < argv.Length && !this.IsOption(argv[_optind + 1])) {
                            _optarg = argv[_optind + 1];
                            nextoffset = 2;
                        } else if (islong && hasarg && curitm.Contains('=')) {
                            _optarg = curitm.Substring(curitm.IndexOf('=') + 1);
                        } else if (islong && hasarg && _optind + 1 < argv.Length && !this.IsOption(argv[_optind + 1])) {
                            _optarg = argv[_optind + 1];
                            nextoffset = 2;
                        } else if (hasarg) {
                            if (curopt.HasArg == ArgFlags.Required) {
                                if (_opterr)
                                    Console.Error.WriteLine("{0}: option '{1}' requires an argument", imgname, curopt.Name);

                                res = optstring.StartsWith(":") ? ':' : '?';
                                _optopt = curopt.Val;
                            } else {
                                /* the argument is optional, so no need to complain */
                                hasarg = false;
                            }
                        }
                    } else {
                        if (_opterr) {
                            string optname = (curchar == '\0') ?
                                curitm :
                                new string(new char[] { curchar });
                            Console.Error.WriteLine("{0}: unrecognised option '{1}'", imgname, optname);
                        }

                        res = '?';
                        _optopt = curchar;
                    }

                    /* support options provided in the same argv element 
                     * ex: -a -b -c becomes -abc */
                    if (!islong && !hasarg && _nextchar < curitm.Length - 2)
                        _nextchar++;
                    else {
                        if (hasarg)     /* don't permute arguments */
                            _optind += nextoffset;
                        else
                            _optind++;
                        _nextchar = 0;
                    }

                    return res;
                } else {  /* not an option */

                    /* strict POSIX mode ends option scanning at the first non-option. + also enables
                     * this mode. - forces non-options to return as character code 1. */
                    if (optstring.StartsWith("+") || this.POSIXLY_CORRECT) {
                        return -1;
                    } else if (optstring.StartsWith("-")) {
                        _optind++;
                        return (char)1;
                    }

                    /* deterimine if there are any options left to read... */
                    bool more = false;
                    for (int i = _optind; i < argv.Length; i++) {
                        if (this.IsOption(argv[i])) {
                            more = true;
                            break;
                        }
                    }

                    /* if not, we're done */
                    if (!more) break;

                    /* permute the argv array throwing the non-option on the end */
                    Array.Copy(argv, _optind + 1, argv, _optind, argv.Length - _optind - 1);
                    argv[argv.Length - 1] = curitm;
                }
            }

            /* no more options to read */
            return -1;
        }

        /// <summary>
        /// Determines if the given string is an option.
        /// </summary>
        /// <param name="opt">the string to inspect</param>
        /// <returns>true when it's an option, false otherwise</returns>
        private bool IsOption(string opt)
        {
            return opt.StartsWith("-") && opt != "-";
        }

        /// <summary>
        /// Determines if the given string is a long option. Note, this should
        /// only be used on known options. Use IsOption() to first deterimine
        /// if the string is an option.
        /// </summary>
        /// <param name="opt">the string to inspect</param>
        /// <returns>true when it's an option, false otherwise</returns>
        private bool IsOptionLong(string opt)
        {
            return opt.StartsWith("--") && opt != "--";
        }

        /// <summary>
        /// Parses the valid options list.
        /// </summary>
        /// <param name="optstring">the options list</param>
        /// <returns>an array of valid option structures</returns>
        private Option[] OptStr2LongOpts(string optstring)
        {
            List<Option> optlst = new List<Option>();

            for (int i = 0; i < optstring.Length; i++) {
                Option curopt = new Option();
                curopt.Name = new String(new char[] { optstring[i] });
                curopt.Val = optstring[i];

                /* one colon means the previous option has an argument, two colons
                 * mean the option has an _optional_ argument */
                if (i + 1 < optstring.Length && optstring[i + 1] == ':') {
                    if (i + 2 < optstring.Length && optstring[i + 2] == ':') {
                        curopt.HasArg = ArgFlags.Optional;
                        i += 2;
                    } else {
                        curopt.HasArg = ArgFlags.Required;
                        i++;
                    }
                }

                optlst.Add(curopt);
            }

            return optlst.ToArray();
        }

        /// <summary>
        /// Index of the next character to read in the current argv element.
        /// </summary>
        public int NextChar
        {
            get { return _nextchar; }
        }

        /// <summary>
        /// Index of the next element to read in the argv array.
        /// </summary>
        public int NextOption
        {
            get { return _optind; }
            set { _optind = value; }
        }

        /// <summary>
        /// Value of the option argument.
        /// </summary>
        public string OptionArg
        {
            get { return _optarg; }
        }

        /// <summary>
        /// Unrecognised option character or option character missing
        /// an argument.
        /// </summary>
        public char OptionChar
        {
            get { return _optopt; }
        }

        /// <summary>
        /// Flag to enable strict POSIX mode.
        /// </summary>
        public bool POSIXLY_CORRECT { get; set; }

        /// <summary>
        /// Flag to toggle printing option scanning errors to stderr.
        /// </summary>
        public bool PrintError
        {
            get { return _opterr; }
            set { _opterr = value; }
        }
    }
}