#include <assimp/Exporter.hpp>
#include <assimp/Importer.hpp>
#include <assimp/scene.h>

#include <algorithm>
#include <iostream>
#include <string>

namespace {
void printNode(const aiNode *node, int depth = 0) {
    std::cout << std::string(depth * 2, ' ') << "node " << node->mName.C_Str()
              << " meshes=" << node->mNumMeshes << " transform=";
    for (unsigned int row = 0; row < 4; ++row) {
        for (unsigned int column = 0; column < 4; ++column) {
            std::cout << node->mTransformation[row][column] << ',';
        }
    }
    std::cout << '\n';
    for (unsigned int i = 0; i < node->mNumChildren; ++i) {
        printNode(node->mChildren[i], depth + 1);
    }
}
} // namespace

int main(int argc, char **argv) {
    if (argc == 3 && std::string(argv[1]) == "--inspect") {
        Assimp::Importer importer;
        const aiScene *scene = importer.ReadFile(argv[2], 0);
        if (scene == nullptr) {
            std::cerr << importer.GetErrorString() << '\n';
            return 5;
        }
        std::cout << "scene meshes=" << scene->mNumMeshes
                  << " materials=" << scene->mNumMaterials
                  << " textures=" << scene->mNumTextures
                  << " animations=" << scene->mNumAnimations << '\n';
        if (scene->mRootNode != nullptr) {
            printNode(scene->mRootNode);
        }
        for (unsigned int mi = 0; mi < scene->mNumMeshes; ++mi) {
            const aiMesh *mesh = scene->mMeshes[mi];
            std::cout << "mesh " << mi << " name=" << mesh->mName.C_Str()
                      << " vertices=" << mesh->mNumVertices
                      << " material=" << mesh->mMaterialIndex
                      << " bones=" << mesh->mNumBones
                      << " uvChannels=" << mesh->GetNumUVChannels()
                      << " morphTargets=" << mesh->mNumAnimMeshes << '\n';
            aiVector3D minimum(1e30f), maximum(-1e30f);
            for (unsigned int vi = 0; vi < mesh->mNumVertices; ++vi) {
                const aiVector3D &v = mesh->mVertices[vi];
                minimum.x = std::min(minimum.x, v.x);
                minimum.y = std::min(minimum.y, v.y);
                minimum.z = std::min(minimum.z, v.z);
                maximum.x = std::max(maximum.x, v.x);
                maximum.y = std::max(maximum.y, v.y);
                maximum.z = std::max(maximum.z, v.z);
            }
            std::cout << "  bounds=" << minimum.x << ',' << minimum.y << ','
                      << minimum.z << ".." << maximum.x << ',' << maximum.y
                      << ',' << maximum.z << '\n';
            for (unsigned int ti = 0; ti < mesh->mNumAnimMeshes; ++ti) {
                std::cout << "  target " << ti << " name="
                          << mesh->mAnimMeshes[ti]->mName.C_Str() << '\n';
            }
        }
        for (unsigned int mi = 0; mi < scene->mNumMaterials; ++mi) {
            const aiMaterial *material = scene->mMaterials[mi];
            aiString name;
            material->Get(AI_MATKEY_NAME, name);
            std::cout << "material " << mi << " name=" << name.C_Str();
            for (aiTextureType type : {aiTextureType_BASE_COLOR,
                                       aiTextureType_DIFFUSE}) {
                for (unsigned int ti = 0;
                     ti < material->GetTextureCount(type);
                     ++ti) {
                    aiString path;
                    unsigned int uvIndex = 0;
                    material->GetTexture(type, ti, &path, nullptr, &uvIndex);
                    std::cout << " texture(type=" << static_cast<int>(type)
                              << ",uv=" << uvIndex << ")=" << path.C_Str();
                }
            }
            std::cout << '\n';
        }
        for (unsigned int ai = 0; ai < scene->mNumAnimations; ++ai) {
            const aiAnimation *animation = scene->mAnimations[ai];
            std::cout << "animation " << ai << " ticksPerSecond="
                      << animation->mTicksPerSecond << " duration="
                      << animation->mDuration << " nodeChannels="
                      << animation->mNumChannels << " morphChannels="
                      << animation->mNumMorphMeshChannels << '\n';
            for (unsigned int ci = 0; ci < animation->mNumMorphMeshChannels; ++ci) {
                const aiMeshMorphAnim *channel = animation->mMorphMeshChannels[ci];
                std::cout << "channel " << ci << " name=" << channel->mName.C_Str()
                          << " keys=" << channel->mNumKeys << '\n';
                for (unsigned int ki = 0; ki < channel->mNumKeys; ++ki) {
                    const aiMeshMorphKey &key = channel->mKeys[ki];
                    std::cout << "  key " << ki << " time=" << key.mTime << " values=";
                    for (unsigned int vi = 0; vi < key.mNumValuesAndWeights; ++vi) {
                        std::cout << key.mValues[vi] << ':' << key.mWeights[vi] << ',';
                    }
                    std::cout << '\n';
                }
            }
        }
        return 0;
    }

    if (argc != 3) {
        std::cerr << "Usage: assimp_morph_fbx_converter input.glb output.fbx\n";
        return 2;
    }

    Assimp::Importer importer;
    const aiScene *scene = importer.ReadFile(argv[1], 0);
    if (scene == nullptr) {
        std::cerr << "Failed to import temporary GLB: " << importer.GetErrorString() << '\n';
        return 3;
    }

    // glTF stores morph target names separately and Assimp can leave the
    // imported aiAnimMesh names empty. FBX requires stable, unique channel
    // names or consumers such as Blender merge/discard their weight curves.
    aiScene *mutableScene = const_cast<aiScene *>(scene);
    for (unsigned int mi = 0; mi < mutableScene->mNumMeshes; ++mi) {
        aiMesh *mesh = mutableScene->mMeshes[mi];
        for (unsigned int ti = 0; ti < mesh->mNumAnimMeshes; ++ti) {
            aiAnimMesh *target = mesh->mAnimMeshes[ti];
            if (target->mName.length == 0) {
                target->mName.Set("morph_" + std::to_string(ti));
            }
        }
    }

    Assimp::Exporter exporter;
    const std::string outputPath = argv[2];
    const bool ascii = outputPath.size() >= 5 &&
                       outputPath.compare(outputPath.size() - 5, 5, ".fbxa") == 0;
    const char *format = ascii ? "fbxa" : "fbx";
    if (exporter.Export(scene, format, outputPath, 0) != AI_SUCCESS) {
        std::cerr << "Failed to export animated FBX: " << exporter.GetErrorString() << '\n';
        return 4;
    }
    return 0;
}
