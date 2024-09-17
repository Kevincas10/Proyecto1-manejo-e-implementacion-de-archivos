using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

public class ArchivoFAT
{
    public string NombreArchivo { get; set; }
    public string RutaDatosInicial { get; set; }
    public bool EnPapelera { get; set; }
    public int CantidadCaracteres { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
    public DateTime FechaEliminacion { get; set; }
}

public class ArchivoDatos
{
    public string Datos { get; set; }
    public string SiguienteArchivo { get; set; }
    public bool EOF { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        bool ejecutando = true;
        while (ejecutando)
        {
            Console.WriteLine("\n--- Menú ---");
            Console.WriteLine("1. Crear archivo");
            Console.WriteLine("2. Listar archivos");
            Console.WriteLine("3. Abrir archivo");
            Console.WriteLine("4. Modificar archivo");
            Console.WriteLine("5. Eliminar archivo");
            Console.WriteLine("6. Recuperar archivo");
            Console.WriteLine("7. Salir");

            Console.Write("Elige una opción: ");
            int opcion = int.Parse(Console.ReadLine());

            switch (opcion)
            {
                case 1:
                    CrearArchivo();
                    break;
                case 2:
                    ListarArchivos();
                    break;
                case 3:
                    AbrirArchivo();
                    break;
                case 4:
                    ModificarArchivo();
                    break;
                case 5:
                    EliminarArchivo();
                    break;
                case 6:
                    RecuperarArchivo();
                    break;
                case 7:
                    ejecutando = false;
                    break;
                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }
    }

    // Opción 1: Crear archivo
    static void CrearArchivo()
    {
        Console.Write("Nombre del archivo: ");
        string nombreArchivo = Console.ReadLine();

        // Capturar nuevo contenido hasta que el usuario presione ESCAPE
        Console.WriteLine("Ingrese el contenido del archivo (presione ESCAPE para terminar):");
        StringBuilder nuevoContenido = new StringBuilder();
        ConsoleKeyInfo tecla;

        do
        {
            tecla = Console.ReadKey(intercept: true);

            if (tecla.Key == ConsoleKey.Backspace && nuevoContenido.Length > 0)
            {
                Console.Write("\b \b");
                nuevoContenido.Remove(nuevoContenido.Length - 1, 1);
            }
            else if (tecla.Key != ConsoleKey.Escape && tecla.Key != ConsoleKey.Enter)
            {
                Console.Write(tecla.KeyChar);
                nuevoContenido.Append(tecla.KeyChar);
            }

        } while (tecla.Key != ConsoleKey.Escape);

        // Confirmación de guardado
        Console.WriteLine("\n¿Desea guardar el archivo? (S/N): ");
        string confirmacion = Console.ReadLine().ToUpper();

        if (confirmacion == "S")
        {
            var fat = new ArchivoFAT
            {
                NombreArchivo = nombreArchivo,
                EnPapelera = false,
                CantidadCaracteres = nuevoContenido.Length,
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now,
                RutaDatosInicial = $"{nombreArchivo}_0.json"
            };

            // Guardar los fragmentos
            GuardarFragmentos(nombreArchivo, nuevoContenido.ToString(), fat);
            Console.WriteLine("Archivo creado exitosamente.");
        }
        else
        {
            Console.WriteLine("Creación del archivo cancelada.");
        }
    }

    // Opción 2: Listar archivos
    static void ListarArchivos()
    {
        var archivosFAT = Directory.GetFiles(Directory.GetCurrentDirectory(), "*_FAT.json");
        foreach (var archivo in archivosFAT)
        {
            var fat = JsonConvert.DeserializeObject<ArchivoFAT>(File.ReadAllText(archivo));
            if (!fat.EnPapelera)
            {
                Console.WriteLine($"Nombre: {fat.NombreArchivo}, Tamaño: {fat.CantidadCaracteres} caracteres, Creado: {fat.FechaCreacion}, Modificado: {fat.FechaModificacion}");
            }
        }
    }

    // Opción 3: Abrir archivo
    static void AbrirArchivo()
    {
        Console.Write("Nombre del archivo a abrir: ");
        string nombreArchivo = Console.ReadLine();

        try
        {
            var fat = JsonConvert.DeserializeObject<ArchivoFAT>(File.ReadAllText($"{nombreArchivo}_FAT.json"));
            string rutaActual = fat.RutaDatosInicial;

            Console.WriteLine($"Nombre: {fat.NombreArchivo}, Tamaño: {fat.CantidadCaracteres} caracteres, Creado: {fat.FechaCreacion}, Modificado: {fat.FechaModificacion}");

            while (!string.IsNullOrEmpty(rutaActual))
            {
                var archivoDatos = JsonConvert.DeserializeObject<ArchivoDatos>(File.ReadAllText(rutaActual));
                Console.Write(archivoDatos.Datos);
                rutaActual = archivoDatos.SiguienteArchivo;
            }

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al abrir el archivo: {ex.Message}");
        }
    }

    // Opción 4: Modificar archivo (ya actualizado)
    static void ModificarArchivo()
    {
        Console.Write("Nombre del archivo a modificar: ");
        string nombreArchivo = Console.ReadLine();

        try
        {
            var fat = JsonConvert.DeserializeObject<ArchivoFAT>(File.ReadAllText($"{nombreArchivo}_FAT.json"));
            string rutaActual = fat.RutaDatosInicial;

            // Mostrar contenido actual
            AbrirArchivo();

            // Capturar nuevo contenido hasta que el usuario presione ESCAPE
            Console.WriteLine("Ingrese el nuevo contenido del archivo (presione ESCAPE para terminar):");
            StringBuilder nuevoContenido = new StringBuilder();
            ConsoleKeyInfo tecla;

            do
            {
                tecla = Console.ReadKey(intercept: true);
                if (tecla.Key == ConsoleKey.Backspace && nuevoContenido.Length > 0)
                {
                    Console.Write("\b \b");
                    nuevoContenido.Remove(nuevoContenido.Length - 1, 1);
                }
                else if (tecla.Key != ConsoleKey.Escape && tecla.Key != ConsoleKey.Enter)
                {
                    Console.Write(tecla.KeyChar);
                    nuevoContenido.Append(tecla.KeyChar);
                }

            } while (tecla.Key != ConsoleKey.Escape);

            // Confirmación para guardar
            Console.WriteLine("\n¿Desea guardar los cambios? (S/N): ");
            string confirmacion = Console.ReadLine().ToUpper();

            if (confirmacion == "S")
            {
                // Eliminar los fragmentos antiguos y guardar nuevos
                GuardarFragmentos(nombreArchivo, nuevoContenido.ToString(), fat);
                Console.WriteLine("Modificación realizada exitosamente.");
            }
            else
            {
                Console.WriteLine("Modificación cancelada.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al modificar el archivo: {ex.Message}");
        }
    }

    // Opción 5: Eliminar archivo
    static void EliminarArchivo()
    {
        Console.Write("Nombre del archivo a eliminar: ");
        string nombreArchivo = Console.ReadLine();

        try
        {
            var fat = JsonConvert.DeserializeObject<ArchivoFAT>(File.ReadAllText($"{nombreArchivo}_FAT.json"));
            Console.WriteLine($"¿Está seguro que desea eliminar el archivo '{nombreArchivo}'? (S/N): ");
            string confirmacion = Console.ReadLine().ToUpper();

            if (confirmacion == "S")
            {
                fat.EnPapelera = true;
                fat.FechaEliminacion = DateTime.Now;
                File.WriteAllText($"{nombreArchivo}_FAT.json", JsonConvert.SerializeObject(fat));
                Console.WriteLine("Archivo eliminado correctamente.");
            }
            else
            {
                Console.WriteLine("Eliminación cancelada.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar el archivo: {ex.Message}");
        }
    }

    // Opción 6: Recuperar archivo
    static void RecuperarArchivo()
    {
        Console.Write("Nombre del archivo a recuperar: ");
        string nombreArchivo = Console.ReadLine();

        try
        {
            var fat = JsonConvert.DeserializeObject<ArchivoFAT>(File.ReadAllText($"{nombreArchivo}_FAT.json"));
            if (fat.EnPapelera)
            {
                Console.WriteLine($"¿Desea recuperar el archivo '{nombreArchivo}'? (S/N): ");
                string confirmacion = Console.ReadLine().ToUpper();

                if (confirmacion == "S")
                {
                    fat.EnPapelera = false;
                    File.WriteAllText($"{nombreArchivo}_FAT.json", JsonConvert.SerializeObject(fat));
                    Console.WriteLine("Archivo recuperado correctamente.");
                }
                else
                {
                    Console.WriteLine("Recuperación cancelada.");
                }
            }
            else
            {
                Console.WriteLine("El archivo no está en la papelera.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al recuperar el archivo: {ex.Message}");
        }
    }

    // Guardar fragmentos en porciones de 20 caracteres
    static void GuardarFragmentos(string nombreArchivo, string contenido, ArchivoFAT fat)
    {
        string rutaActual = fat.RutaDatosInicial;
        int index = 0;

        while (index < contenido.Length)
        {
            string fragmento = contenido.Substring(index, Math.Min(20, contenido.Length - index));
            var archivoDatos = new ArchivoDatos
            {
                Datos = fragmento,
                SiguienteArchivo = (index + 20 < contenido.Length) ? $"{nombreArchivo}_{(index / 20) + 1}.json" : null,
                EOF = (index + 20 >= contenido.Length)
            };

            string rutaFragmento = $"{nombreArchivo}_{(index / 20)}.json";
            File.WriteAllText(rutaFragmento, JsonConvert.SerializeObject(archivoDatos));
            index += 20;
        }

        // Guardar la tabla FAT actualizada
        File.WriteAllText($"{nombreArchivo}_FAT.json", JsonConvert.SerializeObject(fat));
    }
}
