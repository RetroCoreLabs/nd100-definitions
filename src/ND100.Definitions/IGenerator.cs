using System;

namespace NDGen.Core
{
    /// <summary>
    /// Interface for all generators in the NDGen system
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Sets the base directory for the project, to be used for resource resolution
        /// </summary>
        /// <param name="baseDirectory">The root directory of the project</param>
        void SetBaseDirectory(string baseDirectory);
        
        /// <summary>
        /// Executes the generation process
        /// </summary>
        void Generate();
    }
} 