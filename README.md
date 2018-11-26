# Abstrakt

Semestrální projekt z předmětu SMAP (Smart přístupy k tvorbě informačních systémů a aplikací) na Univerzitě Hradec Králové

Žijeme v digitální době, a již s těží bychom našli nějakou činnost, kterou bychom pomocí něj nebyly schopni udělat, nebo si ji minimálně ulehčit. 
Na internetu se registrujeme do stále nových služeb, a v mnohých případech za ty to služby i platíme nemalé peníze. Čím více máme těchto účtů
tím více bychom měli mít i hesel, aby v případě narušení bezpečnosti nebo důvěry v nějaké z nich neměly za následek i kompromitaci ostatních. 
Velké množství hesel je tak zapotřebí nějak rozumně spravovat, z tohoto důvodu již vznikli chytré prográmky pro správu hesel, jako je například keepass,...
Produktem takovéhoto programu je pak, ale binární soubor, který je potřeba nějak chránit proti ztrátě, protože obsahuje velké množství důležitých dat.
Kam rozumně, ale takový důležitý soubor za zálohovat? Pokud jej budeme mít je na jednom počítači, můžeme o něj snadno přijít. Záloha na další fyzické zařízení 
(například flash disk) také není 100 % trvalé a vhodné zálohovací zařízení. Nahrávání takovéhoto souboru do cloudových uložišť je také potencionálně nebezpečné, 
a vyžaduje velkou důvěru vůči poskytovateli tohoto řešení.

## Cíl projektu

Cílem tohoto projektu je vytvořit vhodnou alternativu, pro zálohování těchto relativně malých (předpokládám řádově maximálně desítky MB) souborů, jejichž obsah 
je velice důležitý. Nemusí se jednat jen "databáze hesel", ale mělo by pomocí tohoto řešení jít i zálohovat další důležité binární data, určující naší identitu v 
online světě (různé certifikáty, privátní klíče,...).

Jako velice trvalé médium se zdá být papír, který v mnohých případech, při správném skladování, je schopen přežít i více než 100 let, aniž by se ztratila jediná 
informace na něm zaznamenaná. V rámci tohoto projektu tedy budou zkoumány různé možnosti, jak zálohovat binární počítačová data na toto médium, a jak je pak v 
případě potřeby efektivně (snadno a rychle) obnovit.
