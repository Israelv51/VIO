using VIO.App.Models;

namespace VIO.App.Services;

public static class EmploymentDocumentCatalog
{
    public static List<DocumentSuggestion> Create()
    {
        return
        [
            new()
            {
                Key = "identification",
                Title = "Identificación oficial",
                Category = "Identificación",
                Description = "INE, pasaporte u otra identificación vigente.",
                Group = "Base",
                IsOfficialDocument = true,
                MatchTokens =
                [
                    "ine",
                    "credencial",
                    "identificacion",
                    "identificación",
                    "pasaporte"
                ]
            },

            new()
            {
                Key = "curp",
                Title = "CURP",
                Category = "Documento oficial",
                Description = "Clave Única de Registro de Población.",
                Group = "Base",
                IsOfficialDocument = true,
                MatchTokens = ["curp"]
            },

            new()
            {
                Key = "rfc",
                Title = "RFC / Situación Fiscal",
                Category = "Documento oficial",
                Description = "RFC o Constancia de Situación Fiscal.",
                Group = "Base",
                IsOfficialDocument = true,
                MatchTokens =
                [
                    "rfc",
                    "situacion fiscal",
                    "situación fiscal",
                    "cedula fiscal",
                    "cédula fiscal"
                ]
            },

            new()
            {
                Key = "nss",
                Title = "Número de Seguridad Social",
                Category = "Documento oficial",
                Description = "Documento o constancia con tu NSS del IMSS.",
                Group = "Base",
                IsOfficialDocument = true,
                MatchTokens =
                [
                    "nss",
                    "seguridad social",
                    "imss"
                ]
            },

            new()
            {
                Key = "birth",
                Title = "Acta de nacimiento",
                Category = "Documento oficial",
                Description = "Acta de nacimiento digital o escaneada.",
                Group = "Base",
                IsOfficialDocument = true,
                MatchTokens =
                [
                    "acta de nacimiento",
                    "acta nacimiento"
                ]
            },

            new()
            {
                Key = "address",
                Title = "Comprobante de domicilio",
                Category = "Comprobante",
                Description = "Luz, agua, teléfono, internet u otro comprobante.",
                Group = "Base",
                MatchTokens =
                [
                    "comprobante domicilio",
                    "comprobante de domicilio",
                    "recibo luz",
                    "recibo agua",
                    "recibo internet"
                ]
            },

            new()
            {
                Key = "studies",
                Title = "Comprobante de estudios",
                Category = "Escolar",
                Description = "Constancia, certificado, título o cédula.",
                Group = "Base",
                MatchTokens =
                [
                    "constancia estudios",
                    "constancia de estudios",
                    "certificado estudios",
                    "titulo",
                    "título",
                    "cedula profesional",
                    "cédula profesional"
                ]
            },

            new()
            {
                Key = "cv",
                Title = "Curriculum Vitae",
                Category = "Curriculum Vitae",
                Description = "Tu CV profesional actualizado.",
                Group = "Base",
                MatchTokens =
                [
                    "cv",
                    "curriculum",
                    "currículum"
                ]
            },

            new()
            {
                Key = "bank",
                Title = "Cuenta / CLABE",
                Category = "Comprobante",
                Description = "Carátula o estado de cuenta para depósito de nómina.",
                Group = "Base",
                MatchTokens =
                [
                    "clabe",
                    "estado de cuenta",
                    "caratula bancaria",
                    "carátula bancaria",
                    "cuenta bancaria",
                    "nomina",
                    "nómina"
                ]
            },

            new()
            {
                Key = "certifications",
                Title = "Certificaciones",
                Category = "Certificación",
                Description = "Cursos, certificaciones técnicas o profesionales.",
                Group = "Según puesto",
                MatchTokens =
                [
                    "certificado",
                    "certificacion",
                    "certificación"
                ]
            },

            new()
            {
                Key = "workletter",
                Title = "Constancia laboral",
                Category = "Laboral",
                Description = "Carta o constancia que acredite experiencia previa.",
                Group = "Según puesto",
                MatchTokens =
                [
                    "constancia laboral",
                    "carta laboral"
                ]
            },

            new()
            {
                Key = "recommendation",
                Title = "Carta de recomendación",
                Category = "Laboral",
                Description = "Carta personal o laboral de recomendación.",
                Group = "Según puesto",
                MatchTokens =
                [
                    "recomendacion",
                    "recomendación"
                ]
            },

            new()
            {
                Key = "license",
                Title = "Licencia de conducir",
                Category = "Documento oficial",
                Description = "Para puestos donde sea necesario conducir.",
                Group = "Según puesto",
                IsOfficialDocument = true,
                MatchTokens =
                [
                    "licencia conducir",
                    "licencia de conducir"
                ]
            },

            new()
            {
                Key = "military",
                Title = "Cartilla militar",
                Category = "Documento oficial",
                Description = "Puede solicitarse en ciertos procesos o puestos.",
                Group = "Según puesto",
                IsOfficialDocument = true,
                MatchTokens =
                [
                    "cartilla",
                    "servicio militar"
                ]
            },

            new()
            {
                Key = "criminal",
                Title = "Constancia de no antecedentes",
                Category = "Documento oficial",
                Description = "Solo cuando el puesto o institución la solicite.",
                Group = "Según puesto",
                IsOfficialDocument = true,
                MatchTokens =
                [
                    "antecedentes",
                    "no antecedentes"
                ]
            }
        ];
    }
}
