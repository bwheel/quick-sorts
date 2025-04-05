using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.Drawing;
using System.IO;
using System.Linq;

namespace QuickSorts.App;

public class Program
{
    public static int Main(string[] args)
    {
        RootCommand rootCommand = new RootCommand("Quick Sort a lot of files manuall");
        var options = getCliOptions(
            [
                ("--input", "The path to the directory containing source files"),
                ("--out-confirm", "The path to the directory to send the confirmed files"),
                ("--out-deny", "The path to the directory to send the denied files"),
            ])
            .ToList();
        options.ForEach(o => rootCommand.Add(o));

        rootCommand.SetHandler((inPath, outPathConfirm, outPathDeny) =>
        {
            // Validate inputs
            foreach (var p in new[] { inPath, outPathConfirm, outPathDeny })
                ArgumentException.ThrowIfNullOrWhiteSpace(p, nameof(p));

            if (!Directory.Exists(inPath))
                throw new FileNotFoundException($"missing input folder, {inPath}");

            foreach (var o in new[] { outPathConfirm, outPathDeny })
                if (!Directory.Exists(o))
                    Directory.CreateDirectory(o);

            string[] files = Directory.GetFiles(inPath, "*", SearchOption.AllDirectories);
            if (!files.Any())
            {
                Console.WriteLine("No Files in input folder");
                return;
            }
            int currentIndex = 0;
            int total = files.Length;
            int lastIndex = total - 1;
            Stack<(string, string, string)> movedFiles = new Stack<(string, string, string)>(files.Length);

            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            getStaticFileOptions(
                [
                    (inPath, "/inPath"),
                    (outPathConfirm, "/outPathConfirm"),
                    (outPathDeny, "/outPathDeny")
                ])
            .Select(o => app.UseStaticFiles(o))
            .ToList();

            app.MapGet("/", () => Results.Redirect("/static/index.html"));
            app.MapGet("/total", () => total.ToString());
            app.MapGet("/progress", () => currentIndex.ToString());
            app.MapGet("/curr", () => Path.Combine("inPath", Path.GetRelativePath(inPath, files[currentIndex])));
            app.MapGet("/next", ([FromQuery] string action) =>
            {
                string srcFile = files[currentIndex];
                string relativePath = Path.GetRelativePath(inPath, srcFile);
                string destFile = (action == "confirm")
                    ? Path.Combine(outPathConfirm, relativePath)
                    : Path.Combine(outPathDeny, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Move(srcFile, destFile, overwrite: true);
                string bucket = (action == "confirm") ? "outPathConfirm" : "outPathDeny";
                string path = (action == "confirm") ? outPathConfirm : outPathDeny;
                movedFiles.Push((bucket, path, destFile));
                currentIndex++;
                if (currentIndex >= lastIndex)
                    return "Done";
                string nextFile = files[currentIndex];
                return Path.Combine("inPath", Path.GetRelativePath(inPath, nextFile));
            });
            app.MapGet("/prev", () =>
            {
                if (!movedFiles.Any())
                    return null;
                // Get src/dest files w/ paths
                (string bucket, string path, string prevFile) = movedFiles.Pop();
                string relativePath = Path.GetRelativePath(path, prevFile);
                string destFile = Path.Combine(inPath, relativePath);
                // Move back to src
                File.Move(prevFile, destFile);
                // decrement index
                if (currentIndex > 0)
                    currentIndex--;
                else
                    currentIndex = 0;
                return Path.Combine("inPath", Path.GetRelativePath(inPath, files[currentIndex]));
            });

            app.Run();

        }, options[0], options[1], options[2]);
        return rootCommand.Invoke(args);
    }


    static IEnumerable<Option<string>> getCliOptions((string, string)[] nameAndDescriptions)
    => nameAndDescriptions
        .Select(t => new Option<string>(
            name: t.Item1,
            description: t.Item2
        ));
    static IEnumerable<StaticFileOptions> getStaticFileOptions((string, string)[] inputFolders)
        => inputFolders
            .Select(t => new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(t.Item1),
                RequestPath = t.Item2
            })
            .Append(new StaticFileOptions
            {
                FileProvider = new ManifestEmbeddedFileProvider(typeof(Program).Assembly),
            });
}
