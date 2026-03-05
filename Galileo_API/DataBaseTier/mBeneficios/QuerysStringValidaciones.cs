using System.Text;

namespace Galileo_API.DataBaseTier.mBeneficios
{
    public static class QuerysStringValidaciones
    {
        public const string registroVal = "REGISTRO";
        public const string pagoVal = "PAGO";

        // Placeholders esperados dentro de query_val (plantillas en BD)
        public const string CodCategoriaPlaceholder = "@cod_categoria";
        public const string CodBeneficioPlaceholder = "@cod_beneficio";
        public const string CedulaPlaceholder = "@cedula";
        public const string UsuarioPlaceholder = "@usuario";
        public const string IdBeneficioPlaceholder = "@id_beneficio";
        public const string MontoUsuarioPlaceholder = "@monto_usuario";
        public const string SepelioIdentificacionPlaceholder = "@sepelio_identificacion";

        // Queries "globales" sin categoría (cuando cod_beneficio es null)
        public const string registroP = @"
        SELECT *
        FROM AFI_BENE_VALIDACIONES
        WHERE ESTADO = 1
          AND TIPO = 'P'
          AND REGISTRO = 1
        ORDER BY PRIORIDAD ASC";

        public const string pagoP = @"
        SELECT *
        FROM AFI_BENE_VALIDACIONES
        WHERE ESTADO = 1
          AND PAGO = 1
          AND TIPO = 'P'
        ORDER BY PRIORIDAD ASC";

        /// <summary>
        /// Resuelve la query principal según: tipo, col (REGISTRO/PAGO) y si hay cod_beneficio.
        /// Mantiene la misma tabla de decisión que ya estabas usando.
        /// </summary>
        public static string ResolveQuery(string tipo, string col, string? cod_beneficio)
        {
            bool hasBeneficio = !string.IsNullOrWhiteSpace(cod_beneficio);
            bool isRegistro = col == registroVal;
            string t = (tipo ?? "").Trim().ToUpperInvariant();

            return (hasBeneficio, isRegistro, t) switch
            {
                // SIN cod_beneficio
                (false, true, "P") => registroP,
                (false, true, _) => BuildCategoriaQuery(tipo: "G", col: registroVal, incluirPagoJustifica: false, incluirRegistroJustifica: false),

                (false, false, "P") => pagoP,
                (false, false, "G") => BuildCategoriaQuery(tipo: "P", col: registroVal, incluirPagoJustifica: false, incluirRegistroJustifica: false), // mantengo tu caso raro
                (false, false, _) => BuildCategoriaQuery(tipo: "!G", col: pagoVal, incluirPagoJustifica: true, incluirRegistroJustifica: false),

                // CON cod_beneficio
                (true, true, "P") => BuildCategoriaQuery(tipo: "P", col: registroVal, incluirPagoJustifica: false, incluirRegistroJustifica: false),
                (true, true, _) => BuildCategoriaQuery(tipo: "G", col: registroVal, incluirPagoJustifica: false, incluirRegistroJustifica: false),

                (true, false, "P") => BuildCategoriaQuery(tipo: "P", col: pagoVal, incluirPagoJustifica: false, incluirRegistroJustifica: false),
                (true, false, _) => BuildCategoriaQuery(tipo: "G", col: pagoVal, incluirPagoJustifica: false, incluirRegistroJustifica: false),
            };
        }

        /// <summary>
        /// Builder único para las queries por categoría.
        /// - tipo: "P", "G" o "!G" (equivale a TIPO != 'G')
        /// - col: "REGISTRO" o "PAGO"
        /// - incluirPagoJustifica / incluirRegistroJustifica: agrega columnas si las necesitas en el SELECT
        /// </summary>
        public static string BuildCategoriaQuery(
            string tipo,
            string col,
            bool incluirPagoJustifica,
            bool incluirRegistroJustifica)
        {
            var sb = new StringBuilder();

            sb.Append("select abv.*");

            if (incluirPagoJustifica) sb.Append(", c.pago_justifica");
            if (incluirRegistroJustifica) sb.Append(", c.registro_justifica");

            sb.AppendLine();
            sb.AppendLine("FROM AFI_BENE_VALIDA_CATEGORIA c");
            sb.AppendLine("left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL");
            sb.AppendLine("WHERE COD_CATEGORIA =");
            sb.AppendLine("(");
            sb.AppendLine("  SELECT ab.COD_CATEGORIA");
            sb.AppendLine("  FROM AFI_BENEFICIOS ab");
            sb.AppendLine("  WHERE ab.COD_BENEFICIO = @cod_beneficio");
            sb.AppendLine(")");
            sb.AppendLine("AND c.ESTADO = 1");

            string t = (tipo ?? "").Trim().ToUpperInvariant();
            if (t == "P") sb.AppendLine("AND TIPO = 'P'");
            else if (t == "G") sb.AppendLine("AND TIPO = 'G'");
            else if (t == "!G") sb.AppendLine("AND TIPO != 'G'");

            bool isRegistro = col == registroVal;
            bool isPago = col == pagoVal;

            if (isRegistro) sb.AppendLine("AND REGISTRO = 1");
            if (isPago) sb.AppendLine("AND PAGO = 1");

            sb.AppendLine("order by abv.PRIORIDAD asc");

            return sb.ToString();
        }
    }
}
