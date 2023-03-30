﻿// Copyright (c) 2023 Quetzal Rivera.
// Licensed under the MIT License, See LICENCE in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Vite.AspNetCore.Abstractions;

namespace Vite.AspNetCore.Services
{
	/// <summary>
	/// This class is used to read the manifest.json file generated by Vite.
	/// </summary>
	public sealed class ViteManifest : IViteManifest
	{
		private readonly ILogger<ViteManifest> _logger;
		private readonly IReadOnlyDictionary<string, ViteChunk> _chunks;

		/// <summary>
		/// Initializes a new instance of the <see cref="ViteManifest"/> class.
		/// </summary>
		public ViteManifest(ILogger<ViteManifest> logger, IConfiguration configuration, IWebHostEnvironment environment)
		{
			this._logger = logger;
			// Read the Vite options from the configuration.
			var viteOptions = configuration.GetSection(ViteOptions.Vite).Get<ViteOptions>();

			// Read tha name of the manifest file from the configuration.
			var manifest = viteOptions.Manifest;

			// Get the manifest.json file path
			var manifestPath = Path.Combine(environment.WebRootPath, manifest);

			// If the manifest.json file exists, deserialize it into a dictionary.
			if (File.Exists(manifestPath))
			{
				// Read the manifest.json file and deserialize it into a dictionary
				this._chunks = JsonSerializer.Deserialize<IReadOnlyDictionary<string, ViteChunk>>(File.ReadAllBytes(manifestPath), new JsonSerializerOptions()
				{
					PropertyNameCaseInsensitive = true,
				})!;
			}
			else
			{
				logger.LogWarning("The manifest file was not found. Did you forget 'npm run build'?. Ignore this message if you're using Vite Dev Server.");
				// Create an empty dictionary.
				this._chunks = new Dictionary<string, ViteChunk>();
			}
		}

		/// <summary>
		/// Gets the Vite chunk for the specified entry point if it exists.
		/// If Dev Server is enabled, this will always return <see langword="null"/>.
		/// </summary>
		/// <param name="key"></param>
		/// <returns>The chunk if it exists, otherwise <see langword="null"/>.</returns>
		public IViteChunk? this[string key]
		{
			get
			{
                if (this._chunks.TryGetValue(key, out var chunk))
                {
					this._logger.LogWarning($"The chunk '{key}' was not found. If you're using Vite Dev Server, the manifest service will always return null chunks.");
					return chunk;
                }

				return chunk;
			}
		}
	}
}
