using Xunit;

namespace ApiTienda.Tests
{
    public class EntornoTests
    {
        [Fact]
        public void VerificacionInstalacion_DebeRetornarVerdadero()
        {
            // Arrange (Preparar): Definimos una condición simple
            bool sistemaInstalado = true;

            // Act (Actuar): No hay lógica compleja, solo evaluamos la variable

            // Assert (Afirmar): Si xUnit funciona, esta prueba pasará a verde
            Assert.True(sistemaInstalado, "El motor de pruebas xUnit no está respondiendo correctamente.");
        }
    }
}
