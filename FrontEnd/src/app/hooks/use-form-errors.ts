import { useState, useCallback } from 'react';
import { ValidationError, ApiResponse } from '@/types/api';

export function useFormErrors(initialFields: string[] = []) {
  const [fieldErrors, setFieldErrors] = useState<ValidationError[]>([]);
  const [businessErrors, setBusinessErrors] = useState<string[] | null>(null);
  
  const [queriedProperties] = useState<Set<string>>(new Set(initialFields.map(f => f.toLowerCase())));

  const getMessageFor = (propertyName: string) => {
    queriedProperties.add(propertyName.toLowerCase());
    
    return fieldErrors.find(e => 
      e.propertyName.toLowerCase().includes(propertyName.toLowerCase())
    )?.message;
  };

  const handleResultErrors = useCallback((result: ApiResponse<unknown>) => {
    setFieldErrors(result.errors);
    
    const errorsNotBoundToUi: string[] = [];
    
    if (result.errorMessage)
    {      
      errorsNotBoundToUi.push(result.errorMessage)
    }

    if (result.errors.length > 0)
    {
      const listadoErrores = result.errors
          .filter(e => !e.propertyName || !Array.from(queriedProperties).some(prop => e.propertyName.toLowerCase().includes(prop)))
          .map(e => e.message);
      errorsNotBoundToUi.push(...listadoErrores)
    }

    if (errorsNotBoundToUi.length > 0) {        
      setBusinessErrors(errorsNotBoundToUi);
    } else {
      setBusinessErrors(null);
    }
  }, [queriedProperties]);

  const clearErrors = useCallback(() => {
    setFieldErrors([]);
    setBusinessErrors(null);
  }, []);

  return { handleResultErrors, getMessageFor, businessErrors, fieldErrors, clearErrors };
}