function Factorial(Number: Integer): Integer;
begin
  if Number <= 1 then
    Result := 1
  else
    Result := Number * Factorial(Number - 1);
end;