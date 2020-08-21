using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using VoxelCraft.Rendering;

namespace VoxelCraft.Engine.Rendering
{
    public class FontData
    {
        public int TextureId;
        public Dictionary<char, CharacterData> characterData = new Dictionary<char, CharacterData>();

        public struct CharacterData
        {
            public Vector2 Offset;
            public Vector2 TopLeft;
            public Vector2 BottomRight;
            public float Height;
            public float Width;
            public float Advance;
        }

        public FontData(string textureLocation, string fontLocation, int resolution)
        {
            TextureId = RenderDataHandler.LoadTexture(textureLocation);

            ParseFontFile(fontLocation, resolution);
        }

        private void ParseFontFile(string location, int resolution)
        {
            if (File.Exists(location) == false)
            {
                Debug.Log($"Error while reading font file: File does not exist at path '{location}' !");
                return;
            }

            using FileStream stream = new FileStream(location, FileMode.Open);
            using StreamReader reader = new StreamReader(stream);
            // Read these lines, simply dumping the results.
            // These lines are just data we don't need and can get rid of.
            for (int i = 0; i < 3; i++)
            {
                reader.ReadLine();
            }

            string characterCount = reader.ReadLine();
            if (characterCount.Contains("chars count="))
            {
                int linesToRead = int.Parse(characterCount.Substring(12));

                string readLine;
                string[] splitResult;
                Queue<int> readData = new Queue<int>();
                for (int i = 0; i < linesToRead; i++)
                {
                    readLine = reader.ReadLine();

                    splitResult = Regex.Split(readLine, @"\D+");

                    int addedCount = 0;
                    for (int z = 0; z < splitResult.Length; z++)
                    {
                        // We only want the first 8 entries on a line, any more is garbage for us
                        if (addedCount == 8)
                            break;

                        // Make sure the string isn't empty, and add the value to the read data
                        if (!string.IsNullOrEmpty(splitResult[z]))
                        {
                            addedCount++;
                            readData.Enqueue(int.Parse(splitResult[z]));
                        }
                    }
                }

                // While we have enough data to construct a data entry
                while (readData.Count >= 8)
                {
                    char character = (char)readData.Dequeue();
                    float texTopLeftX = readData.Dequeue() / (float)resolution;
                    float texTopLeftY = readData.Dequeue() / (float)resolution;
                    float width = readData.Dequeue();
                    float height = readData.Dequeue();
                    float xOffset = readData.Dequeue();
                    float yOffset = readData.Dequeue();
                    float xAdvance = readData.Dequeue();

                    characterData.Add(character, new CharacterData()
                    {
                        TopLeft = new Vector2(texTopLeftX, texTopLeftY),
                        BottomRight = new Vector2(width / resolution + texTopLeftX, height / resolution + texTopLeftY),
                        Offset = new Vector2(xOffset, yOffset),
                        Height = height,
                        Width = width,
                        Advance = xAdvance
                    });
                }
            }
            else
            {
                Debug.Log($"Error while reading font file: File is invalid, aborting!");
                return;
            }
        }
    }
}
