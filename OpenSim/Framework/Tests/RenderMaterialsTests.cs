using NUnit.Framework;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace OpenSim.Framework.Tests
{
    [TestFixture]
    public class RenderMaterialsTests
    {
        [TestFixtureSetUp]
        public void Init()
        {
        }

        [Test]
        public void RenderMaterial_OSDFromToTest()
        {
            RenderMaterial mat = new RenderMaterial(UUID.Zero, UUID.Zero, Color4.White);
            OSD map = mat.GetOSD();
            RenderMaterial matFromOSD = RenderMaterial.FromOSD(map);
            Assert.That(mat, Is.EqualTo(matFromOSD));
            Assert.That(matFromOSD.NormalID, Is.EqualTo(UUID.Zero));
            Assert.That(matFromOSD.NormalOffsetX, Is.EqualTo(0.0f));
            Assert.That(matFromOSD.NormalOffsetY, Is.EqualTo(0.0f));
            Assert.That(matFromOSD.NormalRepeatX, Is.EqualTo(1.0f));
            Assert.That(matFromOSD.NormalRepeatY, Is.EqualTo(1.0f));
            Assert.That(matFromOSD.NormalRotation, Is.EqualTo(0.0f));
            Assert.That(matFromOSD.SpecularID, Is.EqualTo(UUID.Zero));
            Assert.That(matFromOSD.SpecularOffsetX, Is.EqualTo(0.0f));
            Assert.That(matFromOSD.SpecularOffsetY, Is.EqualTo(0.0f));
            Assert.That(matFromOSD.SpecularRepeatX, Is.EqualTo(1.0f));
            Assert.That(matFromOSD.SpecularRepeatY, Is.EqualTo(1.0f));
            Assert.That(matFromOSD.SpecularRotation, Is.EqualTo(0.0f));
            Assert.That(matFromOSD.SpecularLightColor, Is.EqualTo(RenderMaterial.DEFAULT_SPECULAR_LIGHT_COLOR));
            Assert.That(matFromOSD.SpecularLightExponent, Is.EqualTo(RenderMaterial.DEFAULT_SPECULAR_LIGHT_EXPONENT));
            Assert.That(matFromOSD.EnvironmentIntensity, Is.EqualTo(RenderMaterial.DEFAULT_ENV_INTENSITY));
            Assert.That(matFromOSD.DiffuseAlphaMode, Is.EqualTo((byte)RenderMaterial.eDiffuseAlphaMode.DIFFUSE_ALPHA_MODE_BLEND));
            Assert.That(matFromOSD.AlphaMaskCutoff, Is.EqualTo(0));
        }

        [Test]
        public void RenderMaterial_ToFromOSDPreservesValues()
        {
            RenderMaterial mat = new RenderMaterial();

            mat.NormalID = UUID.Random();
            mat.NormalOffsetX = 2.0f;
            mat.NormalOffsetY = 2.0f;
            mat.NormalRepeatX = 2.0f;
            mat.NormalRepeatY = 2.0f;
            mat.NormalRotation = 180.0f;

            mat.SpecularID = UUID.Random();
            mat.SpecularOffsetX = 2.0f;
            mat.SpecularOffsetY = 2.0f;
            mat.SpecularRepeatX = 2.0f;
            mat.SpecularRepeatY = 2.0f;
            mat.SpecularRotation = 180.0f;

            mat.SpecularLightColor = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            mat.SpecularLightExponent = 2;
            mat.EnvironmentIntensity = 2;
            mat.DiffuseAlphaMode = (byte)RenderMaterial.eDiffuseAlphaMode.DIFFUSE_ALPHA_MODE_MASK;
            mat.AlphaMaskCutoff = 2;

            OSD map = mat.GetOSD();
            RenderMaterial newmat = RenderMaterial.FromOSD(map);

            Assert.That(newmat, Is.EqualTo(mat));
        }

        [Test]
        public void RenderMaterial_SerializationPreservesValues()
        {
            RenderMaterial mat = new RenderMaterial();

            mat.NormalID = UUID.Random();
            mat.NormalOffsetX = 2.0f;
            mat.NormalOffsetY = 2.0f;
            mat.NormalRepeatX = 2.0f;
            mat.NormalRepeatY = 2.0f;
            mat.NormalRotation = 180.0f;

            mat.SpecularID = UUID.Random();
            mat.SpecularOffsetX = 2.0f;
            mat.SpecularOffsetY = 2.0f;
            mat.SpecularRepeatX = 2.0f;
            mat.SpecularRepeatY = 2.0f;
            mat.SpecularRotation = 180.0f;

            mat.SpecularLightColor = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            mat.SpecularLightExponent = 2;
            mat.EnvironmentIntensity = 2;
            mat.DiffuseAlphaMode = (byte)RenderMaterial.eDiffuseAlphaMode.DIFFUSE_ALPHA_MODE_MASK;
            mat.AlphaMaskCutoff = 2;

            byte[] bytes = mat.ToBytes();
            RenderMaterial newmat = RenderMaterial.FromBytes(bytes, 0, bytes.Length);

            Assert.That(newmat, Is.EqualTo(mat));
        }

        [Test]
        public void RenderMaterial_ToFromBinaryTest()
        {
            RenderMaterial mat = new RenderMaterial();
            RenderMaterials mats = new RenderMaterials();
            UUID key = mats.AddMaterial(mat);

            byte[] bytes = mats.ToBytes();
            RenderMaterials newmats = RenderMaterials.FromBytes(bytes, 0);

            Assert.DoesNotThrow(() =>
            {
                RenderMaterial newmat = newmats.GetMaterial(key);
                Assert.That(mat, Is.EqualTo(newmat));
            });


        }

        [Test]
        public void RenderMaterial_ColorValueToFromMaterialTest()
        {
            RenderMaterial mat = new RenderMaterial();
            mat.SpecularLightColor = new Color4(0.5f, 0.5f, 0.5f, 1.0f);

            byte[] bytes = mat.ToBytes();
            RenderMaterial newmat = RenderMaterial.FromBytes(bytes, 0, bytes.Length);

            Assert.That(mat, Is.EqualTo(newmat));
            Assert.That(mat.SpecularLightColor, Is.EqualTo(new Color4(0.5f, 0.5f, 0.5f, 1.0f)));
        }

        [Test]
        public void RenderMaterial_CopiedMaterialGeneratesTheSameMaterialID()
        {
            RenderMaterial mat = new RenderMaterial();
            RenderMaterial matCopy = (RenderMaterial)mat.Clone();

            UUID matID = RenderMaterial.GenerateMaterialID(mat);
            UUID matCopyID = RenderMaterial.GenerateMaterialID(matCopy);

            Assert.That(mat, Is.EqualTo(matCopy));
            Assert.That(matID, Is.EqualTo(matCopyID));
        }

        [Test]
        public void RenderMaterial_DefaultConstructedMaterialsGeneratesTheSameMaterialID()
        {
            RenderMaterial mat = new RenderMaterial();
            RenderMaterial mat2 = new RenderMaterial();

            UUID matID = RenderMaterial.GenerateMaterialID(mat);
            UUID mat2ID = RenderMaterial.GenerateMaterialID(mat2);

            Assert.That(mat, Is.EqualTo(mat2));
            Assert.That(matID, Is.EqualTo(mat2ID));
        }

        [Test]
        public void RenderMaterial_SerializedMaterialGeneratesTheSameMaterialID()
        {
            RenderMaterial mat = new RenderMaterial();
            UUID matID = new UUID(mat.ComputeMD5Hash(), 0);
            byte[] matData = mat.ToBytes();

            RenderMaterial newmat = RenderMaterial.FromBytes(matData, 0, matData.Length);
            UUID newmatID = RenderMaterial.GenerateMaterialID(newmat);

            Assert.That(mat, Is.EqualTo(newmat));
            Assert.That(matID, Is.EqualTo(newmatID));
        }

        [Test]
        public void RenderMaterial_SerializedMaterialsDataGeneratesTheSameMaterialID()
        {
            RenderMaterials materials = new RenderMaterials();
            RenderMaterial mat = new RenderMaterial();
            UUID matID = materials.AddMaterial(mat);

            byte[] matData = materials.ToBytes();
            RenderMaterials newMaterials = RenderMaterials.FromBytes(matData, 0, matData.Length);
            Assert.That(materials, Is.EqualTo(newMaterials));

            Assert.DoesNotThrow(() =>
            {
                RenderMaterial newmat = newMaterials.GetMaterial(matID);
                UUID newmatID = RenderMaterial.GenerateMaterialID(newmat);
                Assert.That(mat, Is.EqualTo(newmat));
                Assert.That(matID, Is.EqualTo(newmatID));
            });
        }

        [Test]
        public void RenderMaterial_DifferentMaterialsGeneratesDifferentMaterialID()
        {
            RenderMaterial mat = new RenderMaterial();
            RenderMaterial mat2 = new RenderMaterial();
            mat2.NormalID = UUID.Random();

            Assert.AreNotEqual(mat, mat2);
            UUID matID = RenderMaterial.GenerateMaterialID(mat);
            UUID mat2ID = RenderMaterial.GenerateMaterialID(mat2);
            Assert.AreNotEqual(matID, mat2ID);
        }
    }
}