using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Library
{
    public class DBOperationException : Exception
    {
        public DBOperationException(string table) : base($"Operation on {table} thrown no exceptions, but affected unexpected amount of strings.")
        {
        }

        public DBOperationException(string table, Exception inner) : base($"Operation on {table} thrown no exceptions, but affected unexpected amount of strings.", inner)
        {
        }
    }

    public class IntermediateStorageException : Exception
    {
        public IntermediateStorageException() : base("Operation on database was successfull, but same operation on intermediate storage failed for some reason." +
            "This may lead to irrelevant result on retrieving entries, thus should not be swallowed.")
        {
        }

        public IntermediateStorageException(Exception inner) : base("Operation on database was successfull, but same operation on intermediate storage failed for some reason." +
            "This may lead to irrelevant result on retrieving entries, thus should not be swallowed.", inner)
        {
        }
    }
}
