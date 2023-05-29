using System.Collections;
using System;
using UnityEngine;

namespace Meshes
{
    partial struct UMesh
    {

        /// <summary>
        /// Unimplemented
        /// </summary>
        /// <param name="vStart"></param>
        /// <param name="eSplit"></param>
        /// <param name="eNew"></param>
        /// <returns></returns>
        private Vertex EulerSplitEdgeCreateVertex(Vertex vStart, Edge eSplit, out Edge eNew)
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Unimplemented
        /// </summary>
        /// <param name="eKill"></param>
        /// <param name="vKill"></param>
        private void EulerJoinEdgeKillVertex(Edge eKill, Vertex vKill)
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Unimplemented
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private Face SplitFaceCreateEdge(Face face)
        {
            throw new NotImplementedException { };
        }
    }
}