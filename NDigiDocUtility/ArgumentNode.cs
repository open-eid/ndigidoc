// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDigiDocUtility
{
    /// <summary>
    /// This class represents a signgle user argument which can depend on other arguments.
    /// </summary>
    internal class ArgumentNode
    {
        private readonly string _argumentName;
        private readonly bool _isValueSingleton;
        private readonly ArgumentNode[] _dependencies;

        // ** Readonly's

        /// <summary>
        /// Name of the argument
        /// </summary>
        public string ArgumentName { get { return _argumentName; } }
        /// <summary>
        /// Is multiple values of this argument allowed
        /// </summary>
        public bool IsValueSingleton { get { return _isValueSingleton; } }
        /// <summary>
        /// Arguments on which this Argument depends on
        /// </summary>
        public ArgumentNode[] Dependencies { get { return _dependencies; } }

        // ** FFA to change

        /// <summary>
        /// Value of the argument
        /// </summary>
        public string[] ArgumentValues { get; set; }
        /// <summary>
        /// By default, false
        /// </summary>
        public bool IsOptional { get; set; }

        public bool HasValues
        {
            get { return HasElements(ArgumentValues); }
        }

        public bool HasDependencies
        {
            get { return HasElements(Dependencies); }
        }

        private ArgumentNode()
        
        {
        }

        public ArgumentNode(string argumentName, bool isValueSingleton, params ArgumentNode[] depenencies)
        {
            _argumentName = argumentName;
            _isValueSingleton = isValueSingleton;
            _dependencies = depenencies;
        }

        private bool HasElements<T>(T[] array)
        {
            if(array == null || array.Length == 0)
            {
                return false;
            }

            return true;
        }
    }
}
