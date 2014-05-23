// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDigiDocUtility.Operations
{
    internal abstract class AbstractOperation
    {
        /// <summary>
        /// Identifies, if args specify an operation of this instance. Use this method to determine, whether Execute() is required or not.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal virtual bool Validate(params string[] args)
        {
            var requiredDependencies = (Argument.Dependencies == null) ? new ArgumentNode[0] : Argument.Dependencies.Where(x => !x.IsOptional).ToArray();

            // Iterate over all the user args and see if this operation fits the profile as an operation that the user wants to execute.
            // Sets up boolean flags for every specified dependency, that this operation requires. Same for the ArgumentNode object, that contains those dependencies.
            bool operationFound = false;
            var dependenciesFound = new bool[requiredDependencies.Length];
            for(int i = 0; i != args.Length; ++i)
            {
                // See if any parameters correspond to the argument name
                if(args[i].Equals(Argument.ArgumentName))
                {
                    operationFound = true;
                }

                // See if any parameters correspond to the required dependencies of this operation instance
                for(int j = 0; j != requiredDependencies.Length; ++j)
                {
                    if(requiredDependencies[j].ArgumentName.Equals(args[i]))
                    {
                        dependenciesFound[j] = true;
                    }
                }
            }

            // If any of the required dependencies or the operation name is unresolved, this is not
            // considered as an operation that the user wants to execute.
            if(dependenciesFound.Contains(false) || !operationFound)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Uses user arguments to fill current arg node graph values.
        /// </summary>
        /// <param name="args"></param>
        internal virtual void FillArgumentNodeGraphValues(params string[] args)
        {
            // Get current node with dependencies in a single array
            var dep = Argument.Dependencies;
            var allNodes = (dep == null || dep.Length == 0) ? new ArgumentNode[1] : new ArgumentNode[dep.Length + 1];
            allNodes[0] = Argument;
            for (int i = 0; i != allNodes.Length - 1; ++i)
            {
                allNodes[(i + 1)] = Argument.Dependencies[i];
            }

            // Iterate over all nodes
            for (int allNodesIndex = 0; allNodesIndex != allNodes.Length; ++allNodesIndex)
            {
                var currentNode = allNodes[allNodesIndex];
                List<string> currentNodeValues = null;
                // Iterate over all user input args. If user input arg == current node name,
                // start reading in provided values for the current node.
                bool hasValuesForCurrentNode = false; // Notification, that proceeding values belong to this argument until a new argument name is met. Argument name starts with '-'.
                for (int currentArgIndex = 0; currentArgIndex != args.Length; ++currentArgIndex)
                {
                    var currentArg = args[currentArgIndex];
                    if (hasValuesForCurrentNode)
                    {
                        if (currentNodeValues == null)
                        {
                            currentNodeValues = new List<string>();
                        }

                        // If hasValuesForCurrentNode is set to true, we have already encountered a corresponding parameter name and are now reading in values for given node.
                        // If we reach another argument name (arg. names start with '-'), we have no more values to read for current node.
                        if (currentArg.StartsWith("-"))
                        {
                            break;
                        }

                        currentNodeValues.Add(currentArg);
                    }

                    // If current node name == current arg name
                    if (currentNode.ArgumentName.Equals(currentArg))
                    {
                        hasValuesForCurrentNode = true;
                    }
                } // for2

                if (currentNodeValues == null && !currentNode.IsOptional)
                {
                    throw new ArgumentNullException("No parameter values provided for argument '" + currentNode.ArgumentName + "'");
                }

                if(currentNodeValues != null && (currentNode.IsValueSingleton && currentNodeValues.Count > 1))
                {
                    var sb = new StringBuilder(currentNodeValues.Count*20);
                    currentNodeValues.ForEach(x => sb.Append(x).Append(" "));
                    throw new ArgumentOutOfRangeException("Argument '" + currentNode.ArgumentName + "' requires only a single value, but has: " + currentNodeValues.Count + ". Values: " + sb.ToString());
                }

                if (currentNodeValues != null)
                {
                    currentNode.ArgumentValues = currentNodeValues.ToArray();
                }

            } // for1
        }

        /// <summary>
        /// Installs the Argument property.
        /// </summary>
        internal abstract void InstallArgumentNodeGraph();
        internal abstract ArgumentNode Argument { get; }
        internal abstract void Execute();
    }
}
