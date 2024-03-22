namespace meshing;

public unsafe partial class ChunkMesh
{
    protected unsafe bool DrawFaceCommon(Voxel* ptr) => ptr->index == 0;

    protected unsafe bool DrawFaceXN(int j, Voxel* voxelData, bool min, int kCS2)
    {
        // If it is outside this chunk, get the voxel from the neighbouring chunk
        if (min)
        {
            // If no chunk next to us, render
            if (!chunkXN->Allocated)
                return true;

            return DrawFaceCommon(chunkXN->voxels + Constants.ChunkSizeM1 * Constants.ChunkSize + j + kCS2);
        }

        return DrawFaceCommon(voxelData - Constants.ChunkSize);
    }


    protected unsafe bool DrawFaceXP(int j, Voxel* voxelData, bool max, int kCS2)
    {
        if (max)
        {
            // If no chunk next to us, render
            if (!chunkXP->Allocated)
                return true;

            return DrawFaceCommon(chunkXP->voxels + j + kCS2);
        }

        return DrawFaceCommon(voxelData + Constants.ChunkSize);
    }


    protected unsafe bool DrawFaceYN(Voxel* voxelData, bool min, int iCS, int kCS2)
    {
        if (min)
        {
            // If there's no chunk below us, render
            if (!chunkYN->Allocated)
                return true;

            return DrawFaceCommon(chunkYN->voxels + iCS + Constants.ChunkSizeM1 + kCS2);
        }

        return DrawFaceCommon(voxelData - 1);
    }


    protected unsafe bool DrawFaceYP(Voxel* voxelData, bool max, int iCS, int kCS2)
    {
        if (max)
        {
            // If there's no chunk above us, render
            if (!chunkYP->Allocated)
                return true;

            return DrawFaceCommon(chunkYP->voxels + iCS + kCS2);
        }

        return DrawFaceCommon(voxelData + 1);
    }


    protected unsafe bool DrawFaceZN(int j, Voxel* voxelData, bool min, int iCS)
    {
        if (min)
        {
            // if there's no chunk next to us, render
            if (!chunkZN->Allocated)
                return true;

            return DrawFaceCommon(chunkZN->voxels + iCS + j + Constants.ChunkSizeM1 * Constants.ChunkSizeSquared);
        }

        return DrawFaceCommon(voxelData - Constants.ChunkSizeSquared);
    }


    protected unsafe bool DrawFaceZP(int j, Voxel* voxelData, bool max, int iCS)
    {
        if (max)
        {
            // If no chunk next to us, render
            if (!chunkZP->Allocated)
                return true;

            return DrawFaceCommon(chunkZP->voxels + iCS + j);
        }

        return DrawFaceCommon(voxelData + Constants.ChunkSizeSquared);
    }
}
