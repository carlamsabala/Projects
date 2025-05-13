function IsPrime(Number: Integer): Boolean;
var
  i: Integer;
begin
  if Number <= 1 then
    Exit(False); // Numbers less than 2 are not prime

  for i := 2 to Trunc(Sqrt(Number)) do
  begin
    if Number mod i = 0 then
      Exit(False); // If divisible by i, not prime
  end;

  Result := True; // Otherwise, it's prime
end;