using UnityEngine;

namespace MRDL.Utility
{
    public static class TextMeshExtensions
    {
        public static Vector3 GetLocalScale (this TextMesh textMesh) {

            Vector3 localScale = Vector3.zero;

            if (textMesh.text == null)
                return localScale;

            string[] splitStrings = textMesh.text.Split(new string[] { System.Environment.NewLine, "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            // Calculate the width of the text using character info
            float widestLine = 0f;
            foreach (string splitString in splitStrings)
            {
                float lineWidth = 0f;
                foreach (char symbol in splitString)
                {
                    CharacterInfo info;
                    if (textMesh.font.GetCharacterInfo(symbol, out info, textMesh.fontSize, textMesh.fontStyle))
                    {
                        lineWidth += info.advance;
                    }
                }
                if (lineWidth > widestLine)
                    widestLine = lineWidth;
            }
            localScale.x = widestLine;

            // Use this to multiply the character size
            Vector3 transformScale = textMesh.transform.localScale;
            localScale.x = (localScale.x * textMesh.characterSize * 0.1f) * transformScale.x;
            localScale.z = transformScale.z;

            // We could calcualte the height based on line height and character size
            // But I've found that method can be flakey and has a lot of magic numbers
            // that may break in future Unity versions
            Vector3 eulerAngles = textMesh.transform.eulerAngles;
            Vector3 rendererScale = Vector3.zero;
            textMesh.transform.rotation = Quaternion.identity;
            rendererScale = textMesh.GetComponent<MeshRenderer>().bounds.size;
            textMesh.transform.eulerAngles = eulerAngles;
            localScale.y = textMesh.transform.worldToLocalMatrix.MultiplyVector(rendererScale).y * transformScale.y;

            return localScale;
        }
    }
}
