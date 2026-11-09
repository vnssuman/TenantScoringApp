import { useEffect, useState, type FormEvent, type ReactElement } from 'react';
import { ArrowLeft, MapPin, Plus, Wallet } from 'lucide-react';
import { getCurrentProfile } from '../../services/profileApi';
import { createProperty, getProperties, updateProperty } from '../../services/propertyApi';
import type { PropertyInput, PropertyRecord, PropertyType } from '../../types/property';

const emptyForm: PropertyInput = {
  property_name: '',
  property_type: 'Apartment',
  address: '',
  city: '',
  state: '',
  monthly_rent: 0,
};

export function OwnerPropertiesPage({ onBack }: { onBack: () => void }): ReactElement {
  const [properties, setProperties] = useState<PropertyRecord[]>([]);
  const [selectedProperty, setSelectedProperty] = useState<PropertyRecord | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [formData, setFormData] = useState<PropertyInput>(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;
    async function loadProperties(): Promise<void> {
      try {
        const profile = await getCurrentProfile();
        const ownerId = profile.userId;
        const ownerProperties = await getProperties(ownerId);
        if (active) {
          setProperties(ownerProperties);
        }
      } catch (reason) {
        if (active) setError(reason instanceof Error ? reason.message : 'Properties could not be loaded.');
      } finally {
        if (active) setLoading(false);
      }
    }
    void loadProperties();
    return () => { active = false; };
  }, []);

  function openCreateForm(): void {
    setSelectedProperty(null);
    setFormData(emptyForm);
    setError('');
    setIsFormOpen(true);
  }

  function openEditForm(property: PropertyRecord): void {
    setSelectedProperty(property);
    setFormData({
      property_name: property.property_name,
      property_type: property.property_type,
      address: property.address,
      city: property.city,
      state: property.state,
      monthly_rent: property.monthly_rent,
    });
    setError('');
    setIsFormOpen(true);
  }

  function closeForm(): void {
    setIsFormOpen(false);
    setSelectedProperty(null);
    setFormData(emptyForm);
    setError('');
  }

  async function saveProperty(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setSaving(true);
    setError('');
    try {
      const saved = selectedProperty
        ? await updateProperty(selectedProperty.id, formData)
        : await createProperty(formData);
      setProperties((current) => selectedProperty
        ? current.map((property) => property.id === saved.id ? saved : property)
        : [...current, saved]);
      closeForm();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Property could not be saved.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <main className="mobile-content properties-page">
      <button className="properties-back" type="button" onClick={isFormOpen ? closeForm : onBack}><ArrowLeft size={17} /> {isFormOpen ? 'Your Properties' : 'Services'}</button>
      {isFormOpen ? (
        <section className="property-form-view" aria-labelledby="property-form-title">
          <p className="mobile-kicker">Owner workspace</p>
          <h1 id="property-form-title" className="property-form-title">{selectedProperty ? 'Edit Property' : 'Add New Property'}</h1>
          <form className="property-form" onSubmit={saveProperty}>
            <label className="property-field property-field-full">Property name<input required value={formData.property_name} placeholder="e.g., Skyline Apartments" onChange={(event) => setFormData({ ...formData, property_name: event.target.value })} /></label>
            <label className="property-field">Property type<select value={formData.property_type} onChange={(event) => setFormData({ ...formData, property_type: event.target.value as PropertyType })}><option>Apartment</option><option>Villa</option><option>Independent House</option></select></label>
            <label className="property-field property-field-full">Address<textarea required rows={3} value={formData.address} onChange={(event) => setFormData({ ...formData, address: event.target.value })} /></label>
            <label className="property-field">City<input required value={formData.city} onChange={(event) => setFormData({ ...formData, city: event.target.value })} /></label>
            <label className="property-field">State<input required value={formData.state} onChange={(event) => setFormData({ ...formData, state: event.target.value })} /></label>
            <label className="property-field property-field-full">Monthly rent (₹)<input required type="number" min="1" step="1" value={formData.monthly_rent || ''} onChange={(event) => setFormData({ ...formData, monthly_rent: Number(event.target.value) })} /></label>
            {selectedProperty && <div className="property-field property-field-full"><span>Occupancy status</span><strong className={`property-vacancy ${selectedProperty.is_vacant ? 'vacant' : 'occupied'}`}>{selectedProperty.is_vacant ? 'Vacant' : 'Occupied'}</strong></div>}
            {error && <p className="properties-error property-field-full" role="alert">{error}</p>}
            <footer className="property-form-actions property-field-full">
              <button className="property-cancel-button" type="button" onClick={closeForm}>Cancel</button>
              <button className="properties-add-button" type="submit" disabled={saving}>{saving ? 'Saving...' : selectedProperty ? 'Save Changes' : 'Add Property'}</button>
            </footer>
          </form>
        </section>
      ) : (
        <>
          <header className="properties-header">
            <div>
              <p className="mobile-kicker">Owner workspace</p>
              <h1>Your Properties ({properties.length})</h1>
            </div>
            <button className="properties-add-button" type="button" onClick={openCreateForm}><Plus size={17} /> Add New Property</button>
          </header>

          {error && <p className="properties-error" role="alert">{error}</p>}
          {loading ? <p className="properties-empty">Loading your properties...</p> : properties.length === 0 ? (
            <section className="properties-empty"><span>No properties added yet.</span><button type="button" onClick={openCreateForm}>Add your first property</button></section>
          ) : (
            <section className="properties-grid" aria-label="Your properties">
              {properties.map((property) => (
                <button className="property-card" key={property.id} type="button" onClick={() => openEditForm(property)}>
                  <span className={`property-type-badge ${property.property_type.toLowerCase().replaceAll(' ', '-')}`}>{property.property_type}</span>
                  <strong className="property-name">{property.property_name}</strong>
                  <span className="property-detail"><MapPin size={16} aria-hidden="true" />{property.address}, {property.city}, {property.state}</span>
                  <span className="property-detail"><Wallet size={16} aria-hidden="true" />{formatRent(property.monthly_rent)}/month</span>
                  <span className={`property-vacancy ${property.is_vacant ? 'vacant' : 'occupied'}`}>{property.is_vacant ? 'Vacant' : 'Occupied'}</span>
                </button>
              ))}
            </section>
          )}
        </>
      )}
    </main>
  );
}

function formatRent(amount: number): string {
  return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(amount);
}