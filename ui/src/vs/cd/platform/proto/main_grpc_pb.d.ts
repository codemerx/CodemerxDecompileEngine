// GENERATED CODE -- DO NOT EDIT!

// package: 
// file: main.proto

import * as main_pb from "./main_pb";
import * as common_pb from "./common_pb";
import * as grpc from "@grpc/grpc-js";

interface IRpcDecompilerService extends grpc.ServiceDefinition<grpc.UntypedServiceImplementation> {
  restoreDecompilationContext: grpc.MethodDefinition<common_pb.Empty, common_pb.Empty>;
  shouldDecompileFile: grpc.MethodDefinition<main_pb.ShouldDecompileFileRequest, main_pb.ShouldDecompileFileResponse>;
  getContextAssembly: grpc.MethodDefinition<main_pb.GetContextAssemblyRequest, main_pb.GetContextAssemblyResponse>;
  getWorkspaceDirectory: grpc.MethodDefinition<common_pb.Empty, main_pb.GetWorkspaceDirectoryResponse>;
  getAssemblyRelatedFilePaths: grpc.MethodDefinition<main_pb.GetAssemblyRelatedFilePathsRequest, main_pb.GetAssemblyRelatedFilePathsResponse>;
  getProjectCreationMetadata: grpc.MethodDefinition<main_pb.GetProjectCreationMetadataRequest, main_pb.GetProjectCreationMetadataResponse>;
  getAllTypeFilePaths: grpc.MethodDefinition<main_pb.GetAllTypeFilePathsRequest, main_pb.GetAllTypeFilePathsResponse>;
  decompileType: grpc.MethodDefinition<main_pb.DecompileTypeRequest, main_pb.DecompileTypeResponse>;
  getMemberReferenceMetadata: grpc.MethodDefinition<main_pb.GetMemberReferenceMetadataRequest, main_pb.GetMemberReferenceMetadataResponse>;
  getMemberDefinitionPosition: grpc.MethodDefinition<main_pb.GetMemberDefinitionPositionRequest, main_pb.Selection>;
  addResolvedAssembly: grpc.MethodDefinition<main_pb.AddResolvedAssemblyRequest, common_pb.Empty>;
  search: grpc.MethodDefinition<main_pb.SearchRequest, main_pb.SearchResultResponse>;
  cancelSearch: grpc.MethodDefinition<common_pb.Empty, common_pb.Empty>;
  getSearchResultPosition: grpc.MethodDefinition<main_pb.GetSearchResultPositionRequest, main_pb.Selection>;
  createProject: grpc.MethodDefinition<main_pb.CreateProjectRequest, main_pb.CreateProjectResponse>;
  clearAssemblyList: grpc.MethodDefinition<common_pb.Empty, common_pb.Empty>;
}

export const RpcDecompilerService: IRpcDecompilerService;

export class RpcDecompilerClient extends grpc.Client {
  constructor(address: string, credentials: grpc.ChannelCredentials, options?: object);
  restoreDecompilationContext(argument: common_pb.Empty, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  restoreDecompilationContext(argument: common_pb.Empty, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  restoreDecompilationContext(argument: common_pb.Empty, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  shouldDecompileFile(argument: main_pb.ShouldDecompileFileRequest, callback: grpc.requestCallback<main_pb.ShouldDecompileFileResponse>): grpc.ClientUnaryCall;
  shouldDecompileFile(argument: main_pb.ShouldDecompileFileRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.ShouldDecompileFileResponse>): grpc.ClientUnaryCall;
  shouldDecompileFile(argument: main_pb.ShouldDecompileFileRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.ShouldDecompileFileResponse>): grpc.ClientUnaryCall;
  getContextAssembly(argument: main_pb.GetContextAssemblyRequest, callback: grpc.requestCallback<main_pb.GetContextAssemblyResponse>): grpc.ClientUnaryCall;
  getContextAssembly(argument: main_pb.GetContextAssemblyRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetContextAssemblyResponse>): grpc.ClientUnaryCall;
  getContextAssembly(argument: main_pb.GetContextAssemblyRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetContextAssemblyResponse>): grpc.ClientUnaryCall;
  getWorkspaceDirectory(argument: common_pb.Empty, callback: grpc.requestCallback<main_pb.GetWorkspaceDirectoryResponse>): grpc.ClientUnaryCall;
  getWorkspaceDirectory(argument: common_pb.Empty, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetWorkspaceDirectoryResponse>): grpc.ClientUnaryCall;
  getWorkspaceDirectory(argument: common_pb.Empty, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetWorkspaceDirectoryResponse>): grpc.ClientUnaryCall;
  getAssemblyRelatedFilePaths(argument: main_pb.GetAssemblyRelatedFilePathsRequest, callback: grpc.requestCallback<main_pb.GetAssemblyRelatedFilePathsResponse>): grpc.ClientUnaryCall;
  getAssemblyRelatedFilePaths(argument: main_pb.GetAssemblyRelatedFilePathsRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetAssemblyRelatedFilePathsResponse>): grpc.ClientUnaryCall;
  getAssemblyRelatedFilePaths(argument: main_pb.GetAssemblyRelatedFilePathsRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetAssemblyRelatedFilePathsResponse>): grpc.ClientUnaryCall;
  getProjectCreationMetadata(argument: main_pb.GetProjectCreationMetadataRequest, callback: grpc.requestCallback<main_pb.GetProjectCreationMetadataResponse>): grpc.ClientUnaryCall;
  getProjectCreationMetadata(argument: main_pb.GetProjectCreationMetadataRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetProjectCreationMetadataResponse>): grpc.ClientUnaryCall;
  getProjectCreationMetadata(argument: main_pb.GetProjectCreationMetadataRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetProjectCreationMetadataResponse>): grpc.ClientUnaryCall;
  getAllTypeFilePaths(argument: main_pb.GetAllTypeFilePathsRequest, callback: grpc.requestCallback<main_pb.GetAllTypeFilePathsResponse>): grpc.ClientUnaryCall;
  getAllTypeFilePaths(argument: main_pb.GetAllTypeFilePathsRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetAllTypeFilePathsResponse>): grpc.ClientUnaryCall;
  getAllTypeFilePaths(argument: main_pb.GetAllTypeFilePathsRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetAllTypeFilePathsResponse>): grpc.ClientUnaryCall;
  decompileType(argument: main_pb.DecompileTypeRequest, callback: grpc.requestCallback<main_pb.DecompileTypeResponse>): grpc.ClientUnaryCall;
  decompileType(argument: main_pb.DecompileTypeRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.DecompileTypeResponse>): grpc.ClientUnaryCall;
  decompileType(argument: main_pb.DecompileTypeRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.DecompileTypeResponse>): grpc.ClientUnaryCall;
  getMemberReferenceMetadata(argument: main_pb.GetMemberReferenceMetadataRequest, callback: grpc.requestCallback<main_pb.GetMemberReferenceMetadataResponse>): grpc.ClientUnaryCall;
  getMemberReferenceMetadata(argument: main_pb.GetMemberReferenceMetadataRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetMemberReferenceMetadataResponse>): grpc.ClientUnaryCall;
  getMemberReferenceMetadata(argument: main_pb.GetMemberReferenceMetadataRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.GetMemberReferenceMetadataResponse>): grpc.ClientUnaryCall;
  getMemberDefinitionPosition(argument: main_pb.GetMemberDefinitionPositionRequest, callback: grpc.requestCallback<main_pb.Selection>): grpc.ClientUnaryCall;
  getMemberDefinitionPosition(argument: main_pb.GetMemberDefinitionPositionRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.Selection>): grpc.ClientUnaryCall;
  getMemberDefinitionPosition(argument: main_pb.GetMemberDefinitionPositionRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.Selection>): grpc.ClientUnaryCall;
  addResolvedAssembly(argument: main_pb.AddResolvedAssemblyRequest, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  addResolvedAssembly(argument: main_pb.AddResolvedAssemblyRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  addResolvedAssembly(argument: main_pb.AddResolvedAssemblyRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  search(argument: main_pb.SearchRequest, metadataOrOptions?: grpc.Metadata | grpc.CallOptions | null): grpc.ClientReadableStream<main_pb.SearchResultResponse>;
  search(argument: main_pb.SearchRequest, metadata?: grpc.Metadata | null, options?: grpc.CallOptions | null): grpc.ClientReadableStream<main_pb.SearchResultResponse>;
  cancelSearch(argument: common_pb.Empty, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  cancelSearch(argument: common_pb.Empty, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  cancelSearch(argument: common_pb.Empty, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  getSearchResultPosition(argument: main_pb.GetSearchResultPositionRequest, callback: grpc.requestCallback<main_pb.Selection>): grpc.ClientUnaryCall;
  getSearchResultPosition(argument: main_pb.GetSearchResultPositionRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.Selection>): grpc.ClientUnaryCall;
  getSearchResultPosition(argument: main_pb.GetSearchResultPositionRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.Selection>): grpc.ClientUnaryCall;
  createProject(argument: main_pb.CreateProjectRequest, callback: grpc.requestCallback<main_pb.CreateProjectResponse>): grpc.ClientUnaryCall;
  createProject(argument: main_pb.CreateProjectRequest, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.CreateProjectResponse>): grpc.ClientUnaryCall;
  createProject(argument: main_pb.CreateProjectRequest, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<main_pb.CreateProjectResponse>): grpc.ClientUnaryCall;
  clearAssemblyList(argument: common_pb.Empty, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  clearAssemblyList(argument: common_pb.Empty, metadataOrOptions: grpc.Metadata | grpc.CallOptions | null, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
  clearAssemblyList(argument: common_pb.Empty, metadata: grpc.Metadata | null, options: grpc.CallOptions | null, callback: grpc.requestCallback<common_pb.Empty>): grpc.ClientUnaryCall;
}
