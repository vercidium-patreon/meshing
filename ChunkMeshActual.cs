namespace meshing;

public unsafe partial class ChunkMesh
{
    const int ACCESS_STEP_Y = 1;
    const int ACCESS_STEP_X = Constants.ChunkSize;
    const int ACCESS_STEP_Z = Constants.ChunkSizeSquared;


    public unsafe void GenerateMesh()
    {
        // Ensure this chunk exists
        Assert(chunk->Allocated);
        Assert(!chunk->fake);


        meshVisiter = MeshVisiterAllocator.Get();


        // Allocate temp storage for this meshing run. Then we copy from this temp storage to the large vertex buffer
        tempMeshData = TempStorageAllocator<VoxelVertex>.Get();
        tempMeshData.PreMeshing();


        // Precalculate Z voxel access
        var zAccess = 0;


        // Get heightmap pointers that we can modify
        var maxYPointer = chunk->maxAltitude;
        var minYPointer = chunk->minAltitude;


        for (int k = 0; k < Constants.ChunkSize; k++)
        {
            // Precalculate X voxel access
            var xAccess = 0;

            for (int i = 0; i < Constants.ChunkSize; i++)
            {
                // Get the min and max bounds for this column
                int j = *minYPointer++;
                int maxJ = *maxYPointer++;


                // Precalculate voxel access
                var access = zAccess + xAccess + j;
                var voxel = chunk->voxels + access;


                // Mesh from the bottom to the top of this column
                for (; j <= maxJ; j++, access++, voxel++)
                    if (voxel->index > 0)
                        CreateRuns(voxel, i, j, k, access, xAccess, zAccess);
                    

                // Update voxel access
                xAccess += Constants.ChunkSize;
            }


            // Update voxel access
            zAccess += Constants.ChunkSizeSquared;
        }
    }


    protected unsafe void CreateRuns(Voxel* voxel, int i, int j, int k, int access, int xAccess, int zAccess)
    {
        Assert(meshVisiter != null);

        // Check if we're on the edge of this chunk
        var minX = i == 0;
        var maxX = i == Constants.ChunkSizeM1;

        var minZ = k == 0;
        var maxZ = k == Constants.ChunkSizeM1;

        var minY = j == 0;
        var maxY = j == Constants.ChunkSizeM1;


        // Precalculate mesh visiters for each face
        int* visitXN, visitXP, visitYN, visitYP, visitZN, visitZP;

        visitXN = meshVisiter.visitXN + access;
        visitXP = meshVisiter.visitXP + access;
        visitYN = meshVisiter.visitYN + access;
        visitYP = meshVisiter.visitYP + access;
        visitZN = meshVisiter.visitZN + access;
        visitZP = meshVisiter.visitZP + access;


        // Precalculate
        var data = chunk->voxels;
        var index = voxel->index;
        var comparison = meshVisiter.Comparison;
        var textureID = index;
        var i1 = i + 1;
        var j1 = j + 1;


        // 'a' refers to the first axis we combine faces along
        // 'b' refers to the second axis we combine faces along
        //      e.g. for Y+ faces, we merge along the X axis, then along the A axis
        //           for X- faces, we merge up along the Y axis, then along the Z axis
        int end_a;
        int length_b;


        // Left (X-)
        if (*visitXN != comparison && DrawFaceXN(j, voxel, minX, zAccess, index))
        {
            var originalXN = visitXN;


            // Remember we've meshed this face
            *visitXN = comparison;
            visitXN += ACCESS_STEP_Y;


            // Combine faces upwards along the Y axis
            var voxelPointer = data + access + ACCESS_STEP_Y;
            var yAccess = j1;

            for (end_a = j1; end_a < Constants.ChunkSize; end_a++)
            {
                if (voxelPointer->index != index ||                              // It's a different kind of voxel
                    !DrawFaceXN(yAccess, voxelPointer, minX, zAccess, index) ||  // This voxel face is covered by another voxel
                    *visitXN == comparison)                                      // We've already meshed this voxel face
                    break;

                // Step upwards
                voxelPointer++;
                yAccess++;

                // Remember we've meshed this face
                *visitXN = comparison;
                visitXN += ACCESS_STEP_Y;
            }


            // Calculate how many voxels we combined along the Y axis
            var length_a = end_a - j1 + 1;


            // Combine faces along the Z axis
            length_b = 1;

            var max_length_b = Constants.ChunkSize - k;
            var netZAccess = zAccess;

            for (int g = 1; g < max_length_b; g++)
            {
                // Go back to where we started, then move g units along the Z axis
                voxelPointer = data + access;
                voxelPointer += ACCESS_STEP_Z * g;
                yAccess = j;


                // Check if the entire row next to us is also the same index and not covered by another block
                bool adjacentRowIsIdentical = true;

                for (var test_a = j; test_a < end_a; test_a++)
                {
                    // No need to check the meshVisiter here as we're combining on this axis for the first time
                    if (voxelPointer->index != index || !DrawFaceXN(yAccess, voxelPointer, minX, netZAccess, index))
                    {
                        adjacentRowIsIdentical = false;
                        break;
                    }

                    voxelPointer++;
                    yAccess++;
                }

                if (!adjacentRowIsIdentical)
                    break;


                // We found a whole row that's valid!
                length_b++;


                // Remember we've meshed these faces
                var tempXN = originalXN;
                tempXN += ACCESS_STEP_Z * g;

                for (int h = 0; h < length_a; h++)
                {
                    *tempXN = comparison;
                    tempXN += ACCESS_STEP_Y;
                }
            }


            tempMeshData.write++->Reset(i, j, k, -1, 0, 0, 0, 0, textureID);
            tempMeshData.write++->Reset(i, j, k + length_b, -1, 0, 0, 0, 1, textureID);
            tempMeshData.write++->Reset(i, j + length_a, k, -1, 0, 0, 1, 0, textureID);

            tempMeshData.write++->Reset(i, j + length_a, k, -1, 0, 0, 0, 0, textureID);
            tempMeshData.write++->Reset(i, j, k + length_b, -1, 0, 0, 0, 1, textureID);
            tempMeshData.write++->Reset(i, j + length_a, k + length_b, -1, 0, 0, 1, 0, textureID);
        }

        // Right (X+)
        if (*visitXP != comparison && DrawFaceXP(j, voxel, maxX, zAccess, index))
        {
            var originalXP = visitXP;


            // Remember we've meshed this face
            *visitXP = comparison;
            visitXP += ACCESS_STEP_Y;


            // Combine faces along the Y axis
            var voxelPointer = data + access + ACCESS_STEP_Y;
            var yAccess = j1;

            for (end_a = j1; end_a < Constants.ChunkSize; end_a++)
            {
                if (voxelPointer->index != index || !DrawFaceXP(yAccess, voxelPointer, maxX, zAccess, index) || *visitXP == comparison)
                    break;

                voxelPointer++;
                yAccess++;

                // Remember we've meshed this face
                *visitXP = comparison;
                visitXP += ACCESS_STEP_Y;
            }


            // Calculate how many voxels we combined along the Y axis
            var length_a = end_a - j1 + 1;


            // Combine faces along the Z axis
            length_b = 1;

            var max_length_b = Constants.ChunkSize - k;
            var netZAccess = zAccess;

            for (int g = 1; g < max_length_b; g++)
            {
                // Go back to where we started, then move g units on the Z axis
                voxelPointer = data + access;
                voxelPointer += ACCESS_STEP_Z * g;
                yAccess = j;


                // Check if the entire row next to us is also the same index and not covered by another block
                bool adjacentRowIsIdentical = true;

                for (var test_a = j; test_a < end_a; test_a++)
                {
                    // No need to check *yp here as we're combining on this axis for the first time
                    if (voxelPointer->index != index || !DrawFaceXP(yAccess, voxelPointer, maxX, netZAccess, index))
                    {
                        adjacentRowIsIdentical = false;
                        break;
                    }

                    voxelPointer++;
                    yAccess++;
                }

                if (!adjacentRowIsIdentical)
                    break;


                // We found a whole row that's valid!
                length_b++;


                // Remember we've meshed these faces
                var tempXP = originalXP;
                tempXP += ACCESS_STEP_Z * g;

                for (int h = 0; h < length_a; h++)
                {
                    *tempXP = comparison;
                    tempXP += ACCESS_STEP_Y;
                }
            }

            tempMeshData.write++->Reset(i + 1, j, k, 1, 0, 0, 0, 0, textureID);
            tempMeshData.write++->Reset(i + 1, j + length_a, k, 1, 0, 0, 0, 1, textureID);
            tempMeshData.write++->Reset(i + 1, j, k + length_b, 1, 0, 0, 1, 0, textureID);

            tempMeshData.write++->Reset(i + 1, j, k + length_b, 1, 0, 0, 0, 0, textureID);
            tempMeshData.write++->Reset(i + 1, j + length_a, k, 1, 0, 0, 0, 1, textureID);
            tempMeshData.write++->Reset(i + 1, j + length_a, k + length_b, 1, 0, 0, 1, 0, textureID);
        }

        // Back (Z-)
        if (*visitZN != comparison && DrawFaceZN(j, voxel, minZ, xAccess, index))
        {
            var originalZN = visitZN;

            // Remember we've meshed this face
            *visitZN = comparison;
            visitZN += ACCESS_STEP_Y;


            // Combine faces along the Y axis
            var voxelPointer = data + access + ACCESS_STEP_Y;
            var yAccess = j1;

            for (end_a = j1; end_a < Constants.ChunkSize; end_a++)
            {
                if (voxelPointer->index != index || !DrawFaceZN(yAccess, voxelPointer, minZ, xAccess, index) || *visitZN == comparison)
                    break;

                voxelPointer++;
                yAccess++;

                // Remember we've meshed this face
                *visitZN = comparison;
                visitZN += ACCESS_STEP_Y;
            }


            // Calculate how many voxels we combined along the Y axis
            var length_a = end_a - j1 + 1;


            // Combine faces along the X axis
            length_b = 1;

            var max_length_b = Constants.ChunkSize - i;
            var netXAccess = xAccess;

            for (int g = 1; g < max_length_b; g++)
            {
                // Go back to where we started, then move g units on the X axis
                voxelPointer = data + access;
                voxelPointer += ACCESS_STEP_X * g;
                yAccess = j;


                // Check if the entire row next to us is also the same index and not covered by another block
                bool adjacentRowIsIdentical = true;

                for (var test_a = j; test_a < end_a; test_a++)
                {
                    // No need to check *yp here as we're combining on this axis for the first time
                    if (voxelPointer->index != index || !DrawFaceZN(yAccess, voxelPointer, minZ, netXAccess, index))
                    {
                        adjacentRowIsIdentical = false;
                        break;
                    }

                    voxelPointer++;
                    yAccess++;
                }

                if (!adjacentRowIsIdentical)
                    break;


                // We found a whole row that's valid!
                length_b++;


                // Remember we've meshed these faces
                var tempZN = originalZN;
                tempZN += ACCESS_STEP_X * g;

                for (int h = 0; h < length_a; h++)
                {
                    *tempZN = comparison;
                    tempZN += ACCESS_STEP_Y;
                }
            }


            tempMeshData.write++->Reset(i, j, k, 0, 0, -1, 0, 0, textureID);
            tempMeshData.write++->Reset(i, j + length_a, k, 0, 0, -1, 0, 1, textureID);
            tempMeshData.write++->Reset(i + length_b, j, k, 0, 0, -1, 1, 0, textureID);

            tempMeshData.write++->Reset(i + length_b, j, k, 0, 0, -1, 0, 0, textureID);
            tempMeshData.write++->Reset(i, j + length_a, k, 0, 0, -1, 0, 1, textureID);
            tempMeshData.write++->Reset(i + length_b, j + length_a, k, 0, 0, -1, 1, 0, textureID);
        }

        // Front (Z+)
        if (*visitZP != comparison && DrawFaceZP(j, voxel, maxZ, xAccess, index))
        {
            var originalZP = visitZP;


            // Remember we've meshed this face
            *visitZP = comparison;
            visitZP += ACCESS_STEP_Y;


            // Combine faces along the Y axis
            var voxelPointer = data + access + ACCESS_STEP_Y;
            var yAccess = j1;

            for (end_a = j1; end_a < Constants.ChunkSize; end_a++)
            {
                if (voxelPointer->index != index || !DrawFaceZP(yAccess, voxelPointer, maxZ, xAccess, index) || *visitZP == comparison)
                    break;

                voxelPointer++;
                yAccess++;

                // Remember we've meshed this face
                *visitZP = comparison;
                visitZP += ACCESS_STEP_Y;
            }


            // Calculate how many voxels we combined along the Y axis
            var length_a = end_a - j1 + 1;


            // Combine faces along the X axis
            length_b = 1;

            var max_length_b = Constants.ChunkSize - i;
            var netXAccess = xAccess;

            for (int g = 1; g < max_length_b; g++)
            {
                // Go back to where we started, then move g units on the X axis
                voxelPointer = data + access;
                voxelPointer += ACCESS_STEP_X * g;
                yAccess = j;


                // Check if the entire row next to us is also the same index and not covered by another block
                bool adjacentRowIsIdentical = true;

                for (var test_a = j; test_a < end_a; test_a++)
                {
                    // No need to check *yp here as we're combining on this axis for the first time
                    if (voxelPointer->index != index || !DrawFaceZP(yAccess, voxelPointer, maxZ, netXAccess, index))
                    {
                        adjacentRowIsIdentical = false;
                        break;
                    }

                    voxelPointer++;
                    yAccess++;
                }

                if (!adjacentRowIsIdentical)
                    break;


                // We found a whole row that's valid!
                length_b++;


                // Remember we've meshed these faces
                var tempZP = originalZP;
                tempZP += ACCESS_STEP_X * g;

                for (int h = 0; h < length_a; h++)
                {
                    *tempZP = comparison;
                    tempZP += ACCESS_STEP_Y;
                }
            }


            tempMeshData.write++->Reset(i, j, k + 1, 0, 0, 1, 0, 0, textureID);
            tempMeshData.write++->Reset(i + length_b, j, k + 1, 0, 0, 1, 0, 1, textureID);
            tempMeshData.write++->Reset(i, j + length_a, k + 1, 0, 0, 1, 1, 0, textureID);

            tempMeshData.write++->Reset(i, j + length_a, k + 1, 0, 0, 1, 0, 0, textureID);
            tempMeshData.write++->Reset(i + length_b, j, k + 1, 0, 0, 1, 0, 1, textureID);
            tempMeshData.write++->Reset(i + length_b, j + length_a, k + 1, 0, 0, 1, 1, 0, textureID);
        }

        // Bottom (Y-)
        if (*visitYN != comparison && DrawFaceYN(voxel, minY, xAccess, zAccess, index))
        {
            var originalYN = visitYN;

            // Remember we've meshed this face
            *visitYN = comparison;
            visitYN += ACCESS_STEP_X;


            // Combine faces along the X axis
            var voxelPointer = data + access + ACCESS_STEP_X;
            var netXAccess = xAccess + ACCESS_STEP_X;

            for (end_a = i1; end_a < Constants.ChunkSize; end_a++)
            {
                if (voxelPointer->index != index || !DrawFaceYN(voxelPointer, minY, netXAccess, zAccess, index) || *visitYN == comparison)
                    break;

                // Remember we've meshed this face
                *visitYN = comparison;
                visitYN += ACCESS_STEP_X;

                // Move 1 unit on the X axis
                voxelPointer += ACCESS_STEP_X;
                netXAccess += ACCESS_STEP_X;
            }


            // Calculate how many voxels we combined along the X axis
            var length_a = end_a - i1 + 1;


            // Combine faces along the Z axis
            length_b = 1;

            var max_length_b = Constants.ChunkSize - k;

            for (int g = 1; g < max_length_b; g++)
            {
                // Go back to where we started, then move g units on the Z axis
                voxelPointer = data + access;
                voxelPointer += ACCESS_STEP_Z * g;
                netXAccess = xAccess;


                // Check if the entire row next to us is also the same index and not covered by another block
                bool adjacentRowIsIdentical = true;

                for (var test_a = i; test_a < end_a; test_a++)
                {
                    // No need to check *yp here as we're combining on this axis for the first time
                    if (voxelPointer->index != index || !DrawFaceYN(voxelPointer, minY, netXAccess, zAccess, index))
                    {
                        adjacentRowIsIdentical = false;
                        break;
                    }

                    voxelPointer += ACCESS_STEP_X;
                    netXAccess += ACCESS_STEP_X;
                }

                if (!adjacentRowIsIdentical)
                    break;


                // We found a whole row that's valid!
                length_b++;


                // Remember we've meshed these faces
                var tempYN = originalYN;
                tempYN += ACCESS_STEP_Z * g;

                for (int h = 0; h < length_a; h++)
                {
                    *tempYN = comparison;
                    tempYN += ACCESS_STEP_X;
                }
            }

            tempMeshData.write++->Reset(i, j, k, 0, -1, 0, 0, 0, textureID);
            tempMeshData.write++->Reset(i + length_a, j, k, 0, -1, 0, 0, 1, textureID);
            tempMeshData.write++->Reset(i, j, k + length_b, 0, -1, 0, 1, 0, textureID);

            tempMeshData.write++->Reset(i, j, k + length_b, 0, -1, 0, 0, 0, textureID);
            tempMeshData.write++->Reset(i + length_a, j, k, 0, -1, 0, 0, 1, textureID);
            tempMeshData.write++->Reset(i + length_a, j, k + length_b, 0, -1, 0, 1, 0, textureID);
        }


        // Top (Y+)
        if (*visitYP != comparison && DrawFaceYP(voxel, maxY, xAccess, zAccess, index))
        {
            var originalYP = visitYP;

            // Remember we've meshed this face
            *visitYP = comparison;
            visitYP += ACCESS_STEP_X;


            // Combine faces along the X axis
            var voxelPointer = data + access + ACCESS_STEP_X;
            var netXAccess = xAccess + ACCESS_STEP_X;

            for (end_a = i1; end_a < Constants.ChunkSize; end_a++)
            {
                if (voxelPointer->index != index || !DrawFaceYP(voxelPointer, maxY, netXAccess, zAccess, index) || *visitYP == comparison)
                    break;

                // Remember we've meshed this face
                *visitYP = comparison;
                visitYP += ACCESS_STEP_X;

                // Move 1 unit on the X axis
                voxelPointer += ACCESS_STEP_X;
                netXAccess += ACCESS_STEP_X;
            }


            // Calculate how many voxels we combined along the X axis
            var length_a = end_a - i1 + 1;


            // Combine faces along the Z axis
            length_b = 1;

            var max_length_b = Constants.ChunkSize - k;

            for (int g = 1; g < max_length_b; g++)
            {
                // Go back to where we started, then move g units on the Z axis
                voxelPointer = data + access;
                voxelPointer += ACCESS_STEP_Z * g;
                netXAccess = xAccess;


                // Check if the entire row next to us is also the same index and not covered by another block
                bool adjacentRowIsIdentical = true;

                for (var test_a = i; test_a < end_a; test_a++)
                {
                    // No need to check *yp here as we're combining on this axis for the first time
                    if (voxelPointer->index != index || !DrawFaceYP(voxelPointer, maxY, netXAccess, zAccess, index))
                    {
                        adjacentRowIsIdentical = false;
                        break;
                    }

                    voxelPointer += ACCESS_STEP_X;
                    netXAccess += ACCESS_STEP_X;
                }

                if (!adjacentRowIsIdentical)
                    break;


                // We found a whole row that's valid!
                length_b++;


                // Remember we've meshed these faces
                var tempYP = originalYP;
                tempYP += ACCESS_STEP_Z * g;

                for (int h = 0; h < length_a; h++)
                {
                    *tempYP = comparison;
                    tempYP += ACCESS_STEP_X;
                }
            }

            tempMeshData.write++->Reset(i, j + 1, k, 0, 1, 0, 0, 0, textureID);
            tempMeshData.write++->Reset(i, j + 1, k + length_b, 0, 1, 0, 0, 1, textureID);
            tempMeshData.write++->Reset(i + length_a, j + 1, k, 0, 1, 0, 1, 0, textureID);

            tempMeshData.write++->Reset(i + length_a, j + 1, k, 0, 1, 0, 0, 0, textureID);
            tempMeshData.write++->Reset(i, j + 1, k + length_b, 0, 1, 0, 0, 1, textureID);
            tempMeshData.write++->Reset(i + length_a, j + 1, k + length_b, 0, 1, 0, 1, 0, textureID);
        }
    }


    protected unsafe bool DrawFaceCommon(Voxel* nextPtr, byte index)
    {
        if (nextPtr->index == 0)
            return true;

        return false;
    }


    protected unsafe bool DrawFaceXN(int j, Voxel* bPointer, bool min, int kCS2, byte index)
    {
        // If it is outside this chunk, get the voxel from the neighbouring chunk
        if (min)
        {
            if (m.ShouldMeshBetweenChunks)
                return true;

            if (!chunkXN->Allocated)
                return true;

            return DrawFaceCommon(chunkXN->voxels + (Constants.ChunkSize - 1) * Constants.ChunkSize + j + kCS2, index);
        }

        return DrawFaceCommon(bPointer - Constants.ChunkSize, index);
    }


    protected unsafe bool DrawFaceXP(int j, Voxel* bPointer, bool max, int kCS2, byte index)
    {
        if (max)
        {
            if (m.ShouldMeshBetweenChunks)
                return true;

            // If no chunk next to us, render
            if (!chunkXP->Allocated)
                return true;

            return DrawFaceCommon(chunkXP->voxels + j + kCS2, index);
        }

        return DrawFaceCommon(bPointer + Constants.ChunkSize, index);
    }


    protected unsafe bool DrawFaceYN(Voxel* bPointer, bool min, int iCS, int kCS2, byte index)
    {
        if (min)
        {
            if (chunkPosY == 0)
            {

            }
            if (m.ShouldMeshBetweenChunks)
                return true;

            // If there's no chunk below us, render the face
            if (!chunkYN->Allocated)
                return true;

            return DrawFaceCommon(chunkYN->voxels + iCS + (Constants.ChunkSize - 1) + kCS2, index);
        }

        return DrawFaceCommon(bPointer - ACCESS_STEP_Y, index);
    }


    protected unsafe bool DrawFaceYP(Voxel* voxelPointer, bool max, int xAccess, int zAccess, byte index)
    {
        if (max)
        {
            if (m.ShouldMeshBetweenChunks)
                return true;

            // If there's no chunk above us, render
            if (!chunkYP->Allocated)
                return true;

            // Check if there's a block in the bottom layer of the chunk above us
            return DrawFaceCommon(chunkYP->voxels + xAccess + zAccess, index);
        }

        // Check if the block above us is the same index
        return DrawFaceCommon(voxelPointer + ACCESS_STEP_Y, index);
    }


    protected unsafe bool DrawFaceZN(int j, Voxel* bPointer, bool min, int iCS, byte index)
    {
        if (min)
        {
            if (m.ShouldMeshBetweenChunks)
                return true;

            // if there's no chunk next to us, render
            if (!chunkZN->Allocated)
                return true;

            return DrawFaceCommon(chunkZN->voxels + iCS + j + (Constants.ChunkSize - 1) * Constants.ChunkSizeSquared, index);
        }

        return DrawFaceCommon(bPointer - Constants.ChunkSizeSquared, index);
    }


    protected unsafe bool DrawFaceZP(int j, Voxel* bPointer, bool max, int iCS, byte index)
    {
        if (max)
        {
            if (m.ShouldMeshBetweenChunks)
                return true;

            // If no chunk next to us, render
            if (!chunkZP->Allocated)
                return true;

            return DrawFaceCommon(chunkZP->voxels + iCS + j, index);
        }

        return DrawFaceCommon(bPointer + Constants.ChunkSizeSquared, index);
    }
}
