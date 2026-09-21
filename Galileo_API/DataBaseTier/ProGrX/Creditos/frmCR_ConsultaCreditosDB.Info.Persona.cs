using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ProGrX.Credito;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public partial class FrmCRConsultaCreditosDB
    {
        #region Info - Persona Detalle

        /// <summary>
        /// Obtiene el detalle de contacto de la persona ejecutando spAFI_Persona_Consulta,
        /// equivalente al bloque btnInfo_Click Index 0 (Contacto) del formulario VB6.
        /// </summary>
        /// <param name="connection">Conexión abierta del portal.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <returns>Lista con el detalle de contacto; vacía si la persona no existe.</returns>
        private static List<AFPersonaDetalleDto> CR_ConsultaCreditos_PersonaDetalle_Obtener(
            SqlConnection connection,
            string cedula)
        {
            var row = connection.QueryFirstOrDefault(
                "exec spAFI_Persona_Consulta @Cedula",
                new { Cedula = cedula });

            if (row is null)
                return new List<AFPersonaDetalleDto>();

            var d = (IDictionary<string, object?>)row;

            return new List<AFPersonaDetalleDto>
            {
                new AFPersonaDetalleDto
                {
                    estadopersona    = Txt(Val(d, "EstadoPersonaDesc")),
                    fechaingreso     = Fec(Val(d, "FechaIngreso")),
                    membresia        = Txt(Val(d, "Membresia")),
                    fecha_nac        = Fec(Val(d, "fecha_nac")),
                    edad             = Num(Val(d, "Edad")),
                    edad_full        = Txt(Val(d, "Edad_Full")),
                    sexo             = Txt(Val(d, "sexo")),
                    genero_desc      = Txt(Val(d, "GeneroDesc")),
                    estadocivil      = Txt(Val(d, "EstadoCivil")),
                    estadocivil_desc = Txt(Val(d, "EstadoCivilDesc")),
                    nacionalidad     = Txt(Val(d, "Nacionalidad")),
                    pais_nac         = Txt(Val(d, "Pais")),
                    pais_residencia  = Txt(Val(d, "PaisResidencia")),
                    provincia        = Txt(Val(d, "ProvinciaDesc")),
                    canton           = Txt(Val(d, "CantonDesc")),
                    distrito         = Txt(Val(d, "DistritoDesc")),
                    direccion        = Txt(Val(d, "direccion")),
                    email_01         = Txt(Val(d, "AF_Email")),
                    email_02         = Txt(Val(d, "Email_02")),
                    facebook         = Txt(Val(d, "Facebook")),
                    twitter          = Txt(Val(d, "Twitter")),
                    linkedin         = Txt(Val(d, "Linkedin"))
                }
            };
        }

        /// <summary>
        /// Obtiene el valor de una columna del recordset sin importar mayúsculas o minúsculas.
        /// </summary>
        /// <param name="d">Fila devuelta por el stored procedure.</param>
        /// <param name="key">Nombre de la columna buscada.</param>
        /// <returns>Valor de la columna o nulo si no existe.</returns>
        private static object? Val(IDictionary<string, object?> d, string key)
        {
            if (d.TryGetValue(key, out var v)) return v;

            return d
                .Where(par => string.Equals(par.Key, key, StringComparison.OrdinalIgnoreCase))
                .Select(par => par.Value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Convierte el valor a texto recortado.
        /// </summary>
        /// <param name="v">Valor original de la columna.</param>
        /// <returns>Texto sin espacios al inicio ni al final.</returns>
        private static string Txt(object? v) =>
            (Convert.ToString(v, CultureInfo.InvariantCulture) ?? string.Empty).Trim();

        /// <summary>
        /// Convierte el valor a fecha o nulo.
        /// </summary>
        /// <param name="v">Valor original de la columna.</param>
        /// <returns>Fecha o nulo si el valor no es una fecha.</returns>
        private static DateTime? Fec(object? v) =>
            v is DateTime f ? f : null;

        /// <summary>
        /// Convierte el valor a entero o cero.
        /// </summary>
        /// <param name="v">Valor original de la columna.</param>
        /// <returns>Entero equivalente o cero si no es convertible.</returns>
        private static int Num(object? v)
        {
            if (v is null) return 0;

            return int.TryParse(Txt(v), out var n) ? n : 0;
        }

        #endregion
    }
}
