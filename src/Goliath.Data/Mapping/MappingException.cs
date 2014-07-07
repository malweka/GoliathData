using System;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// Mapping Exception class
    /// </summary>
    public class MappingException : GoliathDataException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MappingException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MappingException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Mapping serialization exception class
    /// </summary>
    public class MappingSerializationException : MappingException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingSerializationException"/> class.
        /// </summary>
        /// <param name="typeBeingDeserialized">The type being deserialized.</param>
        /// <param name="errorMessage">The error message.</param>
        public MappingSerializationException(Type typeBeingDeserialized, string errorMessage) 
            : base(string.Format("Couldn't read map and serialize {0},\n{1}", typeBeingDeserialized.FullName, errorMessage)) 
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingSerializationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MappingSerializationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
