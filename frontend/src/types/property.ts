export type PropertyType = 'Apartment' | 'Villa' | 'Independent House';

export interface PropertyRecord {
  id: string;
  property_name: string;
  property_type: PropertyType;
  address: string;
  city: string;
  state: string;
  monthly_rent: number;
  is_vacant: boolean;
}

export type PropertyInput = Omit<PropertyRecord, 'id' | 'is_vacant'>;