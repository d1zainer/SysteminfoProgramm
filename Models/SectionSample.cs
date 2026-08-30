namespace SystemProgramm.Models;

// Вид раздела едет вместе с показаниями: страница отбрасывает чужие,
// если пользователь успел уйти на другую вкладку прямо во время тика.
public sealed record SectionSample(SectionKind Section, SectionReading Reading);
