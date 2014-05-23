// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDigiDocUtility.Operations;

namespace NDigiDocUtility
{
    /// <summary>
    /// This class is a handler for AbstractOperation implementers. If it finds a corresponding
    /// operation, the operation is executed. If it does not find a corresponding operation, an exception is thrown.
    /// </summary>
    internal class ArgumentHandler
    {
        /// <summary>
        /// User input arguments
        /// </summary>
        private string[] _args;
        private AbstractOperation[] _operations;

        private ArgumentHandler()
        
        {
        }

        public ArgumentHandler(string[] args, AbstractOperation[] operations)
        {
            _args = args;
            _operations = operations;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <returns></returns>
        public ArgumentHandler Execute()
        {
            int operationIndex = Validate();
            _operations[operationIndex].FillArgumentNodeGraphValues(_args);
            _operations[operationIndex].Execute();
            return this;
        }

        /// <summary>
        /// <para>Checks if at least one non-optional operation is found that corresponds to the provided user arguments and nothing else.</para>
        /// <para>Executes any non-optional operations</para>
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        /// <returns>Index of the operation, which corresponds to the given user args</returns>
        public int Validate()
        {
            int index = -1;
            for (int i = 0; i != _operations.Length; ++i)
            {
                _operations[i].InstallArgumentNodeGraph();
                if(_operations[i].Validate(_args))
                {
                    if(_operations[i].Argument.IsOptional)
                    {
                        _operations[i].FillArgumentNodeGraphValues(_args);
                        _operations[i].Execute();
                    }
                    else
                    {
                        index = i;    
                    }
                }
            }

            if(index != -1)
            {
                return index;
            }

            var sb = new StringBuilder(_args.Length*20);
            _args.ToList().ForEach(x => sb.Append(x).Append(" "));
            throw new InvalidOperationException("No operation implemented for given user arguments :: " + sb.ToString());
        }
    }
}
