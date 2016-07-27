using System;
using System.Collections.Generic;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// Topological sort based on implementation http://tawani.blogspot.com/2009/02/topological-sorting-and-cyclic.html
    /// </summary>
    class MapSorter
    {
        private int[] vertices;
        private int[,] matrix;
        private int numVerts;
        private int[] sortedArray;

        static ILogger logger;
        static MapSorter()
        {
            logger = Logger.GetLogger(typeof(MapSorter));
        }

        public EntityCollection Sort(EntityCollection tableCollection)
        {
            if (tableCollection == null)
                throw new ArgumentNullException("tableCollection");

            if (tableCollection.Count <= 2)
                return tableCollection;

            var count = tableCollection.Count;

            vertices = new int[count];
            matrix = new int[count, count];
            numVerts = 0;
            sortedArray = new int[count];

            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    matrix[i, j] = 0;
                }
            }

            var indexes = new Dictionary<string, int>();

            for (var i = 0; i < count; i++)
            {
                var tbl = tableCollection[i];
                indexes.Add(tbl.TableName, AddVertex(i));
            }

            for (var i = 0; i < count; i++)
            {
                var tbl = tableCollection[i];

                if (tbl.Relations.Count <= 0) continue;

                foreach (var rf in tbl.Relations)
                {
                    if (rf.RelationType == RelationshipType.ManyToOne)
                        AddEdge(indexes[rf.ReferenceTable], i);
                }
            }

            var sortedIndex = Sort();

            var sortedDictionary = new EntityCollection();

            for (var t = sortedIndex.Length; t-- > 0;)
            {
                var indx = sortedIndex[t];
                var tbl = tableCollection[indx];
                logger.Log(LogLevel.Debug, string.Format("Sort index:{0} table:{1}", indx, tbl.Name));
                sortedDictionary.Add(tbl);
            }

            vertices = null;
            matrix = null;
            numVerts = 0;
            sortedArray = null;

            return sortedDictionary;
        }

        private int[] Sort()
        {
            while (numVerts > 0)
            {
                var currentVertex = NoSuccessors();
                if (currentVertex == -1)
                    throw new Exception("Graph has cycles");

                sortedArray[numVerts - 1] = vertices[currentVertex];
                DeleteVertex(currentVertex);
            }

            return sortedArray;
        }

        private int AddVertex(int vertex)
        {
            vertices[numVerts] = vertex;
            numVerts++;
            return numVerts - 1;
        }

        private void AddEdge(int start, int end)
        {
            matrix[start, end] = 1;
        }

        private int NoSuccessors()
        {
            for (var row = 0; row < numVerts; row++)
            {
                var isEdge = false;
                for (var col = 0; col < numVerts; col++)
                {
                    if (matrix[row, col] <= 0) continue;
                    isEdge = true;
                    break;
                }
                if (!isEdge)
                    return row;
            }
            return -1;
        }

        private void DeleteVertex(int delVert)
        {
            if (delVert != numVerts - 1)
            {
                for (var j = delVert; j < numVerts - 1; j++)
                    vertices[j] = vertices[j + 1];

                for (var row = delVert; row < numVerts - 1; row++)
                    MoveRowUp(row, numVerts);

                for (var col = delVert; col < numVerts - 1; col++)
                    MoveColLeft(col, numVerts - 1);
            }
            numVerts--;
        }

        private void MoveRowUp(int row, int length)
        {
            for (var col = 0; col < length; col++)
                matrix[row, col] = matrix[row + 1, col];
        }

        private void MoveColLeft(int col, int length)
        {
            for (var row = 0; row < length; row++)
                matrix[row, col] = matrix[row, col + 1];
        }
    }
}
